using System.Globalization;

namespace AjuntamentSantaMariaMartorelles.Converters;

/// <summary>true si el valor no és null — per mostrar blocs que depenen de dades carregades.</summary>
public class IsNotNullConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is not null;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
