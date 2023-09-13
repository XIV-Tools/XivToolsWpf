// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System.Windows;
using System.Windows.Input;
using XivToolsWpf.DependencyProperties;

public class RelativeSlider : Slider
{
	public static readonly IBind<double> RelativeValueDp = Binder.Register<double, RelativeSlider>(nameof(RelativeValue));
	public static readonly IBind<double> RelativeRangeDp = Binder.Register<double, RelativeSlider>(nameof(RelativeRange), OnRelativeRangeChanged, BindMode.OneWay);

	private double relativeSliderStart;

	public RelativeSlider()
	{
		this.PreviewMouseDown += this.OnPreviewMouseDown;
		this.PreviewMouseUp += this.OnPreviewMouseUp;
		this.PreviewKeyDown += this.OnPreviewKeyDown;
		this.PreviewKeyUp += this.OnPreviewKeyUp;
		this.ValueChanged += this.OnValueChanged;

		this.Value = 0;
	}

	public double RelativeValue
	{
		get => RelativeValueDp.Get(this);
		set => RelativeValueDp.Set(this, value);
	}

	public double RelativeRange
	{
		get => RelativeRangeDp.Get(this);
		set => RelativeRangeDp.Set(this, value);
	}

	private static void OnRelativeRangeChanged(RelativeSlider sender, double value)
	{
		sender.Minimum = -value;
		sender.Maximum = value;
	}

	private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		this.RelativeValue = this.relativeSliderStart + this.Value;
	}

	private void OnPreviewMouseDown(object? sender, MouseButtonEventArgs e)
	{
		this.relativeSliderStart = this.RelativeValue;
	}

	private void OnPreviewMouseUp(object? sender, MouseButtonEventArgs e)
	{
		this.relativeSliderStart = this.RelativeValue;
		this.Value = 0;
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		this.relativeSliderStart = this.RelativeValue;

		if (e.Key == Key.Left)
		{
			this.OnDecreaseSmall();
			e.Handled = true;
		}
		else if (e.Key == Key.Right)
		{
			this.OnIncreaseSmall();
			e.Handled = true;
		}
	}

	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		this.relativeSliderStart = this.RelativeValue;

		if (e.Key == Key.Left || e.Key == Key.Right)
		{
			e.Handled = true;
			this.Value = 0;
		}
	}
}
