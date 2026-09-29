using System.Globalization;

namespace AjuntamentSantaMariaMartorelles.Converters;

/// <summary>Inverteix un bool — útil per deshabilitar controls mentre IsBusy és true.</summary>
public class InvertedBoolConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is bool b ? !b : value;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is bool b ? !b : value;
}
