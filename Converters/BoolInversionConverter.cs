// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;

	[ValueConversion(typeof(bool), typeof(bool))]
	public class BoolInversionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool v = (bool)value;
			return !v;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool v = (bool)value;
			return !v;
		}
	}
}
