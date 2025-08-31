using System;

namespace HistoricWeatherData.Core.Models
{
    public class ApiAuthenticationException : Exception
    {
        public ApiAuthenticationException()
        {
        }

        public ApiAuthenticationException(string message)
            : base(message)
        {
        }

        public ApiAuthenticationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
