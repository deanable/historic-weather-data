using NUnit.Framework;
using Moq;
using HistoricWeatherData.Core.Services.Interfaces;
using HistoricWeatherData.Core.Services.Implementations;
using System.Threading.Tasks;
using HistoricWeatherData.Core.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Moq.Protected;

namespace HistoricWeatherData.Tests
{
    [TestFixture]
    public class WeatherAPIWeatherServiceTests
    {
        private Mock<ILoggingService> _loggingServiceMock;
        private Mock<ISettingsService> _settingsServiceMock;
        private WeatherAPIWeatherService _weatherService;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _loggingServiceMock = new Mock<ILoggingService>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _weatherService = new WeatherAPIWeatherService(_loggingServiceMock.Object, _settingsServiceMock.Object, _httpClient);
        }

        [Test]
        public async Task GetHistoricalWeatherDataAsync_ReturnsError_WhenApiKeyIsNotSet()
        {
            // Arrange
            _settingsServiceMock.Setup(s => s.GetApiKeyAsync("WeatherAPI")).ReturnsAsync(string.Empty);
            var parameters = new WeatherQueryParameters();

            // Act
            var result = await _weatherService.GetHistoricalWeatherDataAsync(parameters);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("API key for WeatherAPI is not set.", result.ErrorMessage);
        }

        [Test]
        public async Task GetHistoricalWeatherDataAsync_ReturnsSuccess_WithValidData()
        {
            // Arrange
            _settingsServiceMock.Setup(s => s.GetApiKeyAsync("WeatherAPI")).ReturnsAsync("fake-api-key");
            var parameters = new WeatherQueryParameters
            {
                Location = new LocationData { Latitude = 10, Longitude = 10 },
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2023, 1, 1)
            };

            var responseJson = @"{ ""forecast"": { ""forecastday"": [{ ""day"": { ""maxtemp_c"": 15.0, ""mintemp_c"": 5.0, ""totalprecip_mm"": 2.5 } }] } }";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _weatherService.GetHistoricalWeatherDataAsync(parameters);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(15.0, result.Data[0].TemperatureMax);
            Assert.AreEqual(5.0, result.Data[0].TemperatureMin);
            Assert.AreEqual(2.5, result.Data[0].Precipitation);
        }
    }
}
