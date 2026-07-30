using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BankingSystem.Desktop.Converters;

/// <summary>
/// Converts a numeric value into a star GridLength, letting two columns/rows
/// size proportionally to their values (used by the dashboard mini-chart).
/// </summary>
public sealed class DoubleToStarGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = value is null ? 0d : System.Convert.ToDouble(value, culture);
        if (number < 0) number = 0;
        return new GridLength(number, GridUnitType.Star);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
