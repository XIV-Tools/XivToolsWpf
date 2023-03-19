// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Themes;

using System.Windows;
using System.Windows.Media;
using XivToolsWpf.Extensions;

public class CustomTheme
{
	public Color ForegroundBrush { get; set; } = Colors.White;
	public Color ForegroundLightBrush { get; set; } = Colors.White;
	public Color BackgroundBrush { get; set; } = Colors.White;
	public Color BackgroundLightBrush { get; set; } = Colors.White;
	public Color TrimBrush { get; set; } = Colors.White;
	public Color ControlBackgroundBrush { get; set; } = Colors.White;

	public void Apply(Application app)
	{
		app.Resources.Set("ForegroundBrush", new SolidColorBrush(this.ForegroundBrush));
		app.Resources.Set("ForegroundLightBrush", new SolidColorBrush(this.ForegroundLightBrush));
		app.Resources.Set("BackgroundBrush", new SolidColorBrush(this.BackgroundBrush));
		app.Resources.Set("BackgroundLightBrush", new SolidColorBrush(this.BackgroundLightBrush));
		app.Resources.Set("TrimBrush", new SolidColorBrush(this.TrimBrush));
		app.Resources.Set("ControlBackgroundBrush", new SolidColorBrush(this.ControlBackgroundBrush));
	}
}