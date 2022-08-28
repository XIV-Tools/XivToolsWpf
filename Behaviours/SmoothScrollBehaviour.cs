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

public class SmoothScrollBehaviour : Behaviour
{
	public SmoothScrollBehaviour(DependencyObject host)
		: base(host)
	{
	}

	public SmoothScrollBehaviour(DependencyObject el, object? value)
		: base(el, value)
	{
	}

	public override void OnLoaded()
	{
		base.OnLoaded();

		ScrollViewer? scrollViewer = this.Host as ScrollViewer;

		if (scrollViewer == null)
			scrollViewer = this.Host.FindChild<ScrollViewer>();

		if (scrollViewer == null)
			return;

		PropertyInfo? property = scrollViewer.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);

		if (property == null)
			return;

		IScrollInfo? scrollInfo = property.GetValue(scrollViewer) as IScrollInfo;

		if (scrollInfo == null)
			return;

		property.SetValue(scrollViewer, new ScrollInfoAdapter(scrollInfo));
		scrollViewer.SourceUpdated += this.OnSourceUpdated;
	}

	private void OnSourceUpdated(object? sender, System.Windows.Data.DataTransferEventArgs e)
	{
	}
}

public class ScrollInfoAdapter : UIElement, IScrollInfo
{
	private const double ScrollLineDelta = 16.0;
	private const double MouseWheelDelta = 160.0;
	private const double MaxScrollDelta = 50;
	private const double ScrollFalloff = 0.15;

	private readonly IScrollInfo original;
	private Task? animationTask;
	private double? targetVerticalOffset;
	private double? targetHorizontalOffset;

	public ScrollInfoAdapter(IScrollInfo original)
	{
		this.original = original;
	}

	public ScrollViewer ScrollOwner
	{
		get => this.original.ScrollOwner;
		set => this.original.ScrollOwner = value;
	}

	public bool CanVerticallyScroll
	{
		get => this.original.CanVerticallyScroll;
		set => this.original.CanHorizontallyScroll = value;
	}

	public bool CanHorizontallyScroll
	{
		get => this.original.CanHorizontallyScroll;
		set => this.original.CanHorizontallyScroll = value;
	}

	public double ExtentWidth => this.original.ExtentWidth;
	public double ExtentHeight => this.original.ExtentHeight;
	public double ViewportWidth => this.original.ViewportWidth;
	public double ViewportHeight => this.original.ViewportHeight;
	public double HorizontalOffset => this.original.HorizontalOffset;
	public double VerticalOffset => this.original.VerticalOffset;

	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		return this.original.MakeVisible(visual, rectangle);
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
		this.original.SetVerticalOffset(offset);
	}

	public void SetHorizontalOffset(double offset)
	{
		this.targetHorizontalOffset = offset;
		this.original.SetHorizontalOffset(offset);
	}

	private static double Lerp(double current, double to)
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

	private void VerticalScroll(double val)
	{
		if (this.targetVerticalOffset == null)
			this.targetVerticalOffset = this.original.VerticalOffset;

		this.targetVerticalOffset = this.targetVerticalOffset + val;
		this.targetVerticalOffset = Math.Clamp((double)this.targetVerticalOffset, 0, this.original.ScrollOwner.ScrollableHeight);
		this.Animate();
	}

	private void HorizontalScroll(double val)
	{
		if (this.targetHorizontalOffset == null)
			this.targetHorizontalOffset = this.original.HorizontalOffset;

		this.targetHorizontalOffset = this.targetHorizontalOffset + val;
		this.targetHorizontalOffset = Math.Clamp((double)this.targetHorizontalOffset, 0, this.original.ScrollOwner.ScrollableWidth);

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
			this.targetHorizontalOffset = this.original.HorizontalOffset;

		if (this.targetVerticalOffset == null)
			this.targetVerticalOffset = this.original.VerticalOffset;

		do
		{
			this.original.SetVerticalOffset(Lerp(this.original.VerticalOffset, (double)this.targetVerticalOffset));
			this.original.SetHorizontalOffset(Lerp(this.original.HorizontalOffset, (double)this.targetHorizontalOffset));
			await Task.Delay(1);
			await Dispatch.MainThread();
		}
		while (this.original.VerticalOffset != this.targetVerticalOffset ||
			this.original.HorizontalOffset != this.targetHorizontalOffset);

		this.targetVerticalOffset = null;
		this.targetHorizontalOffset = null;
	}
}