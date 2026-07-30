using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BankingSystem.Desktop.Converters;

/// <summary>Visible when the bound boolean is false; collapsed when true.</summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility v && v != Visibility.Visible;
}
