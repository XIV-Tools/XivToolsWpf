// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

[ValueConversion(typeof(Enum), typeof(Visibility))]
public class EnumToVisibilityConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return Visibility.Collapsed;

		Type enumType = value.GetType();

		if (!enumType.IsEnum)
			throw new Exception("Enum converter can only be used on an enum type");

		Enum parameterValue = (Enum)Enum.Parse(enumType, (string)parameter);
		Enum currentValue = (Enum)value;

		if (Enum.Equals(currentValue, parameterValue))
			return Visibility.Visible;

		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
