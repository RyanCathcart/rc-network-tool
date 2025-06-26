using System.Globalization;

namespace rc_network_tool.Utils.Converters;

class AdapterSpeedLongToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return "N/A";

        long speed = (long)value;

        if (speed > 1_000_000_000_000)
            return "Very high";

        else if (speed > 1_000_000_000)
            return $"{speed / 1_000_000_000} Gbps";
        
        else if (speed > 1_000_000)
            return $"{speed / 1_000_000} Mbps";
        
        else if (speed > 1_000)
            return $"{speed / 1_000} Kbps";

        else if (speed < 0)
            return $"0 bps";

        else
            return $"{speed} bps";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return null;

        string text = (string)value;

        if (long.TryParse(text.Split(" ")[0], out long speed))
            return speed;
        else
            return null;
    }
}
