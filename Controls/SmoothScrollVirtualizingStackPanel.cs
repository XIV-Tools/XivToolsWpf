// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System.Threading.Tasks;
using System;
using System.Windows.Controls;
using XivToolsWpf.Behaviours;

public class SmoothScrollVirtualizingStackPanel : VirtualizingStackPanel
{
	private Task? animationTask;
	private double? targetVerticalOffset;
	private double? targetHorizontalOffset;

	public override void LineUp() => this.VerticalScroll(-ScrollInfoAdapter.ScrollLineDelta);
	public override void LineDown() => this.VerticalScroll(+ScrollInfoAdapter.ScrollLineDelta);
	public override void LineLeft() => this.HorizontalScroll(-ScrollInfoAdapter.ScrollLineDelta);
	public override void LineRight() => this.HorizontalScroll(+ScrollInfoAdapter.ScrollLineDelta);
	public override void MouseWheelUp() => this.VerticalScroll(-ScrollInfoAdapter.MouseWheelDelta);
	public override void MouseWheelDown() => this.VerticalScroll(+ScrollInfoAdapter.MouseWheelDelta);
	public override void MouseWheelLeft() => this.HorizontalScroll(-ScrollInfoAdapter.MouseWheelDelta);
	public override void MouseWheelRight() => this.HorizontalScroll(+ScrollInfoAdapter.MouseWheelDelta);
	public override void PageUp() => this.VerticalScroll(-this.ViewportHeight);
	public override void PageDown() => this.VerticalScroll(+this.ViewportHeight);
	public override void PageLeft() => this.HorizontalScroll(-this.ViewportWidth);
	public override void PageRight() => this.HorizontalScroll(+this.ViewportWidth);

	private void VerticalScroll(double val)
	{
		if (this.targetVerticalOffset == null)
			this.targetVerticalOffset = this.VerticalOffset;

		this.targetVerticalOffset = this.targetVerticalOffset + val;
		this.targetVerticalOffset = Math.Clamp((double)this.targetVerticalOffset, 0, this.ScrollOwner.ScrollableHeight);
		this.Animate();
	}

	private void HorizontalScroll(double val)
	{
		if (this.targetHorizontalOffset == null)
			this.targetHorizontalOffset = this.HorizontalOffset;

		this.targetHorizontalOffset = this.targetHorizontalOffset + val;
		this.targetHorizontalOffset = Math.Clamp((double)this.targetHorizontalOffset, 0, this.ScrollOwner.ScrollableWidth);

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
			this.targetHorizontalOffset = this.HorizontalOffset;

		if (this.targetVerticalOffset == null)
			this.targetVerticalOffset = this.VerticalOffset;

		do
		{
			this.SetVerticalOffset(ScrollInfoAdapter.Lerp(this.VerticalOffset, (double)this.targetVerticalOffset));
			this.SetHorizontalOffset(ScrollInfoAdapter.Lerp(this.HorizontalOffset, (double)this.targetHorizontalOffset));
			await Task.Delay(1);
			await this.Dispatcher.MainThread();
		}
		while (this.VerticalOffset != this.targetVerticalOffset ||
			this.HorizontalOffset != this.targetHorizontalOffset);

		this.targetVerticalOffset = null;
		this.targetHorizontalOffset = null;
	}
}
