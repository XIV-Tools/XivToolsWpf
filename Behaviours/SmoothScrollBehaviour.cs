// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Behaviours;

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

public class SmoothScrollBehaviour : Behaviour
{
	public static readonly PropertyInfo? ScrollInfoProperty = typeof(ScrollViewer).GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);

	public SmoothScrollBehaviour(DependencyObject host)
		: base(host)
	{
	}

	public SmoothScrollBehaviour(DependencyObject el, object? value)
		: base(el, value)
	{
	}

	public static void SetSmoothScroll(DependencyObject host, bool enable)
	{
		host.AttachHandler<SmoothScrollBehaviour>(enable);
	}

	public override void OnLoaded()
	{
		base.OnLoaded();

		ScrollViewer? scrollViewer = this.Host as ScrollViewer;

		if (scrollViewer == null)
			scrollViewer = this.Host.FindChild<ScrollViewer>();

		if (scrollViewer == null)
			return;

		IScrollInfo? scrollInfo = ScrollInfoProperty?.GetValue(scrollViewer) as IScrollInfo;

		if (scrollInfo == null)
			return;

		ScrollInfoProperty?.SetValue(scrollViewer, new ScrollInfoAdapter(scrollInfo));
	}
}

public class ScrollInfoAdapter : UIElement, IScrollInfo
{
	public const double ScrollLineDelta = 16.0;
	public const double MouseWheelDelta = 160.0;
	public const double MaxScrollDelta = 50;
	public const double ScrollFalloff = 0.15;

	public readonly IScrollInfo Original;

	private Task? animationTask;
	private double? targetVerticalOffset;
	private double? targetHorizontalOffset;

	public ScrollInfoAdapter(IScrollInfo original)
	{
		this.Original = original;
	}

	public ScrollViewer ScrollOwner
	{
		get => this.Original.ScrollOwner;
		set => this.Original.ScrollOwner = value;
	}

	public bool CanVerticallyScroll
	{
		get => this.Original.CanVerticallyScroll;
		set => this.Original.CanHorizontallyScroll = value;
	}

	public bool CanHorizontallyScroll
	{
		get => this.Original.CanHorizontallyScroll;
		set => this.Original.CanHorizontallyScroll = value;
	}

	public double ExtentWidth => this.Original.ExtentWidth;
	public double ExtentHeight => this.Original.ExtentHeight;
	public double ViewportWidth => this.Original.ViewportWidth;
	public double ViewportHeight => this.Original.ViewportHeight;
	public double HorizontalOffset => this.Original.HorizontalOffset;
	public double VerticalOffset => this.Original.VerticalOffset;

	public static double Lerp(double current, double to)
	{
		double delta = to - current;
		delta *= ScrollFalloff;
		delta = Math.Clamp(delta, -MaxScrollDelta, MaxScrollDelta);

		current += delta;

		if (delta < 1 && delta > -1)
		{
			return to;
		}
		else
		{
			return current;
		}
	}

	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		return this.Original.MakeVisible(visual, rectangle);
	}

	public void LineUp() => this.VerticalScroll(-ScrollLineDelta);
	public void LineDown() => this.VerticalScroll(+ScrollLineDelta);
	public void LineLeft() => this.HorizontalScroll(-ScrollLineDelta);
	public void LineRight() => this.HorizontalScroll(+ScrollLineDelta);
	public void MouseWheelUp() => this.VerticalScroll(-MouseWheelDelta);
	public void MouseWheelDown() => this.VerticalScroll(+MouseWheelDelta);
	public void MouseWheelLeft() => this.HorizontalScroll(-MouseWheelDelta);
	public void MouseWheelRight() => this.HorizontalScroll(+MouseWheelDelta);
	public void PageUp() => this.VerticalScroll(-this.ViewportHeight);
	public void PageDown() => this.VerticalScroll(+this.ViewportHeight);
	public void PageLeft() => this.HorizontalScroll(-this.ViewportWidth);
	public void PageRight() => this.HorizontalScroll(+this.ViewportWidth);

	public void SetVerticalOffset(double offset)
	{
		this.targetVerticalOffset = offset;
		this.Original.SetVerticalOffset(offset);
	}

	public void SetHorizontalOffset(double offset)
	{
		this.targetHorizontalOffset = offset;
		this.Original.SetHorizontalOffset(offset);
	}

	private void VerticalScroll(double val)
	{
		if (this.targetVerticalOffset == null)
			this.targetVerticalOffset = this.Original.VerticalOffset;

		this.targetVerticalOffset = this.targetVerticalOffset + val;
		this.targetVerticalOffset = Math.Clamp((double)this.targetVerticalOffset, 0, this.Original.ScrollOwner.ScrollableHeight);
		this.Animate();
	}

	private void HorizontalScroll(double val)
	{
		if (this.targetHorizontalOffset == null)
			this.targetHorizontalOffset = this.Original.HorizontalOffset;

		this.targetHorizontalOffset = this.targetHorizontalOffset + val;
		this.targetHorizontalOffset = Math.Clamp((double)this.targetHorizontalOffset, 0, this.Original.ScrollOwner.ScrollableWidth);

		this.Animate();
	}

	private void Animate()
	{
		if (this.animationTask == null || this.animationTask.IsCompleted)
		{
			this.animationTask = this.AnimateToTarget();
		}
	}

	private async Task AnimateToTarget()
	{
		if (this.targetHorizontalOffset == null)
			this.targetHorizontalOffset = this.Original.HorizontalOffset;

		if (this.targetVerticalOffset == null)
			this.targetVerticalOffset = this.Original.VerticalOffset;

		do
		{
			this.Original.SetVerticalOffset(Lerp(this.Original.VerticalOffset, (double)this.targetVerticalOffset));
			this.Original.SetHorizontalOffset(Lerp(this.Original.HorizontalOffset, (double)this.targetHorizontalOffset));
			await Task.Delay(1);
			await this.Dispatcher.MainThread();
		}
		while (this.Original.VerticalOffset != this.targetVerticalOffset ||
			this.Original.HorizontalOffset != this.targetHorizontalOffset);

		this.targetVerticalOffset = null;
		this.targetHorizontalOffset = null;
	}
}