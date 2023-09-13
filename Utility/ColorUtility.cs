// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Utility;

using System;
using System.Windows.Media;

public static class ColorUtility
{
	// https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm
	public static void FromHsl(ref Color rgb, double h, double s, double l)
	{
		double v;
		double r, g, b;
		r = l;   // default to gray
		g = l;
		b = l;
		v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - (l * s));
		if (v > 0)
		{
			double m;
			double sv;
			int sextant;
			double fract, vsf, mid1, mid2;
			m = l + l - v;
			sv = (v - m) / v;
			h *= 6.0;
			sextant = (int)h;
			fract = h - sextant;
			vsf = v * sv * fract;
			mid1 = m + vsf;
			mid2 = v - vsf;
			switch (sextant)
			{
				case 0:
					r = v;
					g = mid1;
					b = m;
					break;
				case 1:
					r = mid2;
					g = v;
					b = m;
					break;
				case 2:
					r = m;
					g = v;
					b = mid1;
					break;
				case 3:
					r = m;
					g = mid2;
					b = v;
					break;
				case 4:
					r = mid1;
					g = m;
					b = v;
					break;
				case 5:
					r = v;
					g = m;
					b = mid2;
					break;
			}
		}

		rgb.R = Convert.ToByte(r * 255.0f);
		rgb.G = Convert.ToByte(g * 255.0f);
		rgb.B = Convert.ToByte(b * 255.0f);
		rgb.A = 255;
	}

	public static Color FromHsl(double h, double s, double l)
	{
		Color rgb = default;
		FromHsl(ref rgb, h, s, l);
		return rgb;
	}
}
