// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

[ValueConversion(typeof(Color), typeof(Brush))]
public class ColorToBrushConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not Color color)
			return new SolidColorBrush(Colors.Black);

		return new SolidColorBrush(color);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
