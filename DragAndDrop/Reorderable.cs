// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DragAndDrop;

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf.Behaviours;

public class Reorderable : Behaviour
{
	public readonly ItemsControl ItemsControl;

	private DropTargetInsertionAdorner? dragAdorner;

	public Reorderable(ItemsControl control)
		: base(control)
	{
		this.ItemsControl = control;

		this.ItemsControl.ItemContainerGenerator.StatusChanged += this.OnContainerGeneratorStatusChanged;

		INotifyCollectionChanged source = this.ItemsControl.Items;
		source.CollectionChanged += this.OnControlItemsChanged;
	}

	public static void SetIsReorderable(ItemsControl items, bool enable)
	{
		items.AttachHandler<Reorderable>(enable);
	}

	public override void Dispose()
	{
		base.Dispose();

		INotifyCollectionChanged source = this.ItemsControl.Items;
		source.CollectionChanged -= this.OnControlItemsChanged;
	}

	private void OnContainerGeneratorStatusChanged(object? sender, EventArgs e)
	{
		foreach (object? newItem in this.ItemsControl.Items)
		{
			this.AttachToContainerContext(newItem);
		}
	}

	private void OnControlItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
		{
			foreach (object? oldItem in e.OldItems)
			{
				this.DetachFromContainerContext(oldItem);
			}
		}

		if (e.NewItems != null)
		{
			foreach (object? newItem in e.NewItems)
			{
				this.AttachToContainerContext(newItem);
			}
		}
	}

	private void DetachFromContainerContext(object context)
	{
		// The item container is just deleted in this context, so its fine to just let the events die with it.
		////DependencyObject? itemContainer = this.ItemsControl.ItemContainerGenerator.ContainerFromItem(context);
	}

	private void AttachToContainerContext(object context)
	{
		UIElement? itemContainer = this.ItemsControl.ItemContainerGenerator.ContainerFromItem(context) as UIElement;

		if (itemContainer == null)
			return;

		itemContainer.AllowDrop = true;

		itemContainer.PreviewMouseMove -= this.OnMouseMove;
		itemContainer.Drop -= this.OnDrop;
		itemContainer.DragEnter -= this.OnDragEnter;
		itemContainer.DragLeave -= this.OnDragLeave;
		itemContainer.DragOver -= this.OnDragOver;

		itemContainer.PreviewMouseMove += this.OnMouseMove;
		itemContainer.Drop += this.OnDrop;
		itemContainer.DragEnter += this.OnDragEnter;
		itemContainer.DragLeave += this.OnDragLeave;
		itemContainer.DragOver += this.OnDragOver;
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement senderElement)
		{
			Task.Run(async () =>
			{
				await Task.Delay(250);
				await Dispatch.MainThread();

				if (e.LeftButton != MouseButtonState.Pressed)
					return;

				var data = new DataObject();
				data.SetSource(senderElement);
				data.SetContext(senderElement.DataContext);
				System.Windows.DragDrop.DoDragDrop(senderElement, data, DragDropEffects.Move);
				e.Handled = true;
			});
		}
	}

	private void OnDrop(object sender, DragEventArgs e)
	{
		this.dragAdorner?.Detatch();
		this.dragAdorner = null;

		if (sender is not FrameworkElement senderElement)
			return;

		DropTargetInsertionAdorner.InsertPositions insertPosition = this.GetDropPosition(sender, e);
		bool isNext = insertPosition == DropTargetInsertionAdorner.InsertPositions.Right || insertPosition == DropTargetInsertionAdorner.InsertPositions.Bottom;

		IList? source = this.ItemsControl.ItemsSource as IList;

		if (source == null)
			throw new Exception("Items control items source is not an IList");

		object? context = e.GetContext();

		if (context == null)
			throw new Exception("No context in drag");

		source.Remove(context);

		int index = source.IndexOf(senderElement.DataContext);

		if (isNext)
			index++;

		if (index < 0)
			index = 0;

		source.Insert(index, context);
	}

	private void OnDragEnter(object sender, DragEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		object? context = e.GetContext();

		// Cant drag nothing, and cant drag onto ourselves.
		if (context == null || context == senderElement.DataContext)
		{
			e.Effects = DragDropEffects.None;
			return;
		}

		e.Effects = DragDropEffects.Move;
		e.SetTarget(senderElement);

		if (this.dragAdorner == null)
		{
			this.dragAdorner = new(senderElement, e);
		}

		this.OnDragOver(sender, e);
	}

	private void OnDragOver(object sender, DragEventArgs e)
	{
		if (this.dragAdorner == null)
			return;

		if (sender is not FrameworkElement senderElement)
			return;

		DropTargetInsertionAdorner.InsertPositions newPos = this.GetDropPosition(sender, e);

		if (this.dragAdorner.InsertPosition != newPos)
		{
			this.dragAdorner.Detatch();
			this.dragAdorner = new(senderElement, e, newPos);
		}
	}

	private DropTargetInsertionAdorner.InsertPositions GetDropPosition(object sender, DragEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return DropTargetInsertionAdorner.InsertPositions.Left;

		Point dropPoint = e.GetPosition(senderElement);

		bool vertical = false;

		StackPanel? itemParent = senderElement.FindParent<StackPanel>();
		if (itemParent != null)
		{
			vertical = itemParent.Orientation == Orientation.Vertical;
		}

		if (vertical)
		{
			if (dropPoint.Y > (senderElement.ActualHeight / 2))
			{
				return DropTargetInsertionAdorner.InsertPositions.Bottom;
			}
			else
			{
				return DropTargetInsertionAdorner.InsertPositions.Top;
			}
		}
		else
		{
			if (dropPoint.X > (senderElement.ActualWidth / 2))
			{
				return DropTargetInsertionAdorner.InsertPositions.Right;
			}
			else
			{
				return DropTargetInsertionAdorner.InsertPositions.Left;
			}
		}
	}

	private void OnDragLeave(object sender, DragEventArgs e)
	{
		this.dragAdorner?.Detatch();
		this.dragAdorner = null;
	}
}