// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters
{
	using System;
	using System.Windows;
	using System.Windows.Data;

	[ValueConversion(typeof(object), typeof(Visibility))]
	public class NotZeroToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isZero = true;

			if (value is int intV)
			{
				isZero = intV == 0;
			}
			else if (value is float floatV)
			{
				isZero = floatV == 0;
			}
			else if (value is double doubleV)
			{
				isZero = doubleV == 0;
			}
			else if (value is uint uintV)
			{
				isZero = uintV == 0;
			}
			else
			{
				throw new NotImplementedException($"value type {value.GetType()} not supported for not zero converter");
			}

			return isZero ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
