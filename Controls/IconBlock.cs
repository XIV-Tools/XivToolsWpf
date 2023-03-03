// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using FontAwesome.Sharp;
using FontAwesome.Sharp.Pro;

public class IconBlock : IconBlockBase<ProIcons>
{
	public static readonly DependencyProperty IconStyleProperty = DependencyProperty.Register(
		"IconStyle",
		typeof(IconStyles),
		typeof(IconBlock),
		new PropertyMetadata(IconStyles.Solid, OnIconStylePropertyChanged));

	private static readonly FontFamily NoneFont = new();
	private static readonly Typeface? SolidFont;
	private static readonly Typeface? OutlineFont;
	private static readonly Typeface? OutlineThinFont;
	private static readonly Typeface? BrandsFont;

	static IconBlock()
	{
		Assembly assembly = typeof(IconBlock).Assembly;

		IconBlock.SolidFont = new Typeface(assembly.LoadFont("fonts", "Font Awesome 5 Pro Solid"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
		IconBlock.OutlineFont = new Typeface(assembly.LoadFont("fonts", "Font Awesome 5 Pro Regular"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
		IconBlock.OutlineThinFont = new Typeface(assembly.LoadFont("fonts", "Font Awesome 5 Pro Light"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
		IconBlock.BrandsFont = new Typeface(assembly.LoadFont("fonts", "Font Awesome 5 Brands Regular"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
	}

	public IconBlock()
		: base(NoneFont)
	{
	}

	public IconStyles IconStyle
	{
		get => (IconStyles)this.GetValue(IconStyleProperty);
		set => this.SetValue(IconStyleProperty, value);
	}

	protected override FontFamily FontFor(ProIcons icon)
	{
		FontFamily? font = this.FontFor(icon, this.IconStyle);

		if (font == null)
			font = this.FontFor(icon, IconStyles.Solid);

		if (font == null)
			font = this.FontFor(icon, IconStyles.Brand);

		if (font == null)
			return IconBlock.NoneFont;

		return font;
	}

	protected virtual FontFamily? FontFor(ProIcons icon, IconStyles style)
	{
		Typeface? typeface = this.IconStyle switch
		{
			IconStyles.Solid => IconBlock.SolidFont,
			IconStyles.Outline => IconBlock.OutlineFont,
			IconStyles.OutlineThin => IconBlock.OutlineThinFont,
			IconStyles.Brand => IconBlock.BrandsFont,
			_ => throw new NotSupportedException(),
		};

		if (typeface == null)
			throw new NotSupportedException();

		int iconCode = (int)icon;
		ushort glyphIndex;
		GlyphTypeface gt;

		bool glyphFound = false;
		if (typeface.TryGetGlyphTypeface(out gt))
		{
			glyphFound = gt.CharacterToGlyphMap.TryGetValue(iconCode, out glyphIndex);
		}

		if (glyphFound)
			return typeface.FontFamily;

		return null;
	}

	private static void OnIconStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is IconBlock iconBlock)
		{
			ProIcons icon = iconBlock.Icon;
			iconBlock.Icon = ProIcons.None;
			iconBlock.Icon = icon;
		}
	}
}

public enum IconStyles
{
	Solid,
	Outline,
	OutlineThin,
	Brand,
}