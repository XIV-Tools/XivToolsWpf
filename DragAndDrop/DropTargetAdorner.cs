// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DragAndDrop;

using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

public abstract class DropTargetAdorner : Adorner
{
	private readonly AdornerLayer? adornerLayer;

	public DropTargetAdorner(UIElement adornedElement, DragEventArgs args)
		: base(adornedElement)
	{
		this.adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
		this.adornerLayer?.Add(this);
		this.IsHitTestVisible = false;
		this.EventArgs = args;
	}

	public DragEventArgs EventArgs { get; private set; }

	public void Detatch()
	{
		this.adornerLayer?.Remove(this);
	}

	protected sealed override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		this.OnRender(drawingContext, this.EventArgs);
	}

	protected abstract void OnRender(DrawingContext drawingContext, DragEventArgs args);
}
