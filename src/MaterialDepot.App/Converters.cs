using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MaterialDepot.App;

/// <summary>
/// Visible when the value is set (not null, not an empty string, not false); with the parameter <c>Invert</c> the other way round.
/// </summary>
public sealed class PresenceToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var present = value switch
        {
            null => false,
            string text => text.Length > 0,
            bool flag => flag,
            int count => count > 0,
            _ => true,
        };
        var invert = parameter is "Invert";
        return present != invert ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// <see langword="true"/> for <see langword="false"/> and the other way round, for "enabled while not busy".
/// </summary>
public sealed class NegationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;
}
