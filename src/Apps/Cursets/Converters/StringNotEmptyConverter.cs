using System.Globalization;

namespace AjuntamentSantaMariaMartorelles.Converters;

/// <summary>true si el string té contingut — per mostrar/amagar controls opcionals.</summary>
public class StringNotEmptyConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> !string.IsNullOrWhiteSpace(value as string);

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
