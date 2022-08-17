// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters;

using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(Enum), typeof(bool))]
public class EnumToBoolConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return false;

		Type enumType = value.GetType();

		if (!enumType.IsEnum)
			throw new Exception("Enum converter can only be used on an enum type");

		Enum parameterValue = (Enum)Enum.Parse(enumType, (string)parameter);
		Enum currentValue = (Enum)value;

		bool val = Enum.Equals(currentValue, parameterValue);
		return val;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not bool bVal || bVal == false)
			return Binding.DoNothing;

		if (!targetType.IsEnum)
			throw new Exception("Enum converter can only be used on an enum type");

		return Enum.Parse(targetType, (string)parameter);
	}
}
