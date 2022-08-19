// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using FontAwesome.Sharp;

public class IconBlock : IconBlockBase<IconChar>
{
	private static readonly FontFamily NoneFont = new();
	private static Typeface[]? typefaces;

	public IconBlock()
		: base(NoneFont)
	{
	}

	protected override FontFamily FontFor(IconChar icon)
	{
		if (typefaces == null)
			Load();

		GlyphTypeface gt;
		ushort glyphIndex;
		FontFamily? font = typefaces.Find(icon, out gt, out glyphIndex)?.FontFamily;

		if (font == null)
			return typefaces![0].FontFamily;

		return font;
	}

	private static void Load()
	{
		string path = "fonts";

		string[] names = new string[2];
		names[0] = "Font Awesome 5 Pro Solid";
		names[1] = "Font Awesome 5 Brands Regular";

		typefaces = typeof(IconBlock).Assembly.LoadTypefaces(path, names);
	}
}
