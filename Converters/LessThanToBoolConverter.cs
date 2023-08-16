// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters;

using System.Windows;

public class LessThanToBoolConverter : ConverterBase<double, bool, double>
{
	protected override bool Convert(double value)
	{
		return value < this.Parameter;
	}
}
