// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Converters;

public class BoolToIntConverter : ConverterBase<bool, int>
{
	protected override int Convert(bool value)
	{
		return value ? 1 : 0;
	}
}
