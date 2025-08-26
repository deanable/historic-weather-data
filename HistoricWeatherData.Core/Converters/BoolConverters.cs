using System.Globalization;

namespace HistoricWeatherData.Core.Converters
{
    public static class InvertedBoolConverter
    {
        public static object Convert(object? value)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public static object ConvertBack(object? value)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    public static class BoolToColorConverter
    {
        public static bool Convert(object? value)
        {
            if (value is bool boolValue && boolValue)
            {
                return true; // Loading color
            }
            return false; // Default color
        }

        public static bool ConvertBack(object? value)
        {
            return false;
        }
    }

    public static class NullToVisibilityConverter
    {
        public static bool Convert(object? value)
        {
            return value != null;
        }

        public static bool ConvertBack(object? value)
        {
            return value is bool boolValue && boolValue;
        }
    }

    public static class CountToVisibilityConverter
    {
        public static bool Convert(object? value)
        {
            if (value is int count)
            {
                return count > 0;
            }
            return false;
        }

        public static int ConvertBack(object? value)
        {
            return value is bool boolValue && boolValue ? 1 : 0;
        }
    }

    public static class StringToVisibilityConverter
    {
        public static bool Convert(object? value, string compareValue)
        {
            if (value is string stringValue)
            {
                return stringValue == compareValue;
            }
            return false;
        }

        public static string? ConvertBack(object? value, string parameter)
        {
            return parameter;
        }
    }

    public static class StringToBoolConverter
    {
        public static bool Convert(object? value, string compareValue)
        {
            if (value is string stringValue)
            {
                return stringValue == compareValue;
            }
            return false;
        }

        public static string? ConvertBack(object? value, string parameter)
        {
            if (value is bool boolValue && boolValue)
            {
                return parameter;
            }
            return null;
        }
    }
}