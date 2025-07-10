
using System.Globalization;

namespace rc_network_tool.Utils.Converters;

class IsMacChangedBoolToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return "N/A";

        bool isMacChanged = (bool)value;

        return isMacChanged ? "Yes" : "No";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return false;

        string text = (string)value;

        return text.Equals("Yes");
    }
}
