// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Extensions;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

public class FastObservableCollection<T> : ObservableCollection<T>, IFastObservableCollection
{
	private bool suppressChangedEvent = false;

	public void Replace(IEnumerable<T> other)
	{
		this.suppressChangedEvent = true;

		this.Clear();
		this.AddRange(other);
	}

	public void Replace(IEnumerable other)
	{
		this.suppressChangedEvent = true;

		this.Clear();

		foreach (T item in other)
		{
			this.Add(item);
		}

		this.suppressChangedEvent = false;

		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		this.OnPropertyChanged(new(nameof(this.Count)));
	}

	public void AddRange(IEnumerable other)
	{
		this.suppressChangedEvent = true;

		foreach (object item in other)
		{
			if (item is T tItem)
			{
				this.Add(tItem);
			}
		}

		this.suppressChangedEvent = false;

		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		this.OnPropertyChanged(new(nameof(this.Count)));
	}

	public void RemoveRange(IEnumerable other)
	{
		this.suppressChangedEvent = true;

		foreach (object item in other)
		{
			if (item is T tItem)
			{
				this.Remove(tItem);
			}
		}

		this.suppressChangedEvent = false;

		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		this.OnPropertyChanged(new(nameof(this.Count)));
	}

	public void SortAndReplace(IEnumerable<T> other, IComparer<T> comparer)
	{
		List<T> values = new(other);
		values.Sort(comparer);
		this.Replace(values);
	}

	public void Sort(IComparer<T> comparer)
	{
		List<T> values = new(this);
		values.Sort(comparer);
		this.Replace(values);
	}

	public void Synchronize(NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add && args.NewItems != null)
		{
			this.AddRange(args.NewItems);
		}
		else if (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems != null)
		{
			this.RemoveRange(args.OldItems);
		}
		else if (args.Action == NotifyCollectionChangedAction.Reset)
		{
			this.Clear();
		}
	}

	protected override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.suppressChangedEvent)
			return;

		base.OnPropertyChanged(e);
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (this.suppressChangedEvent)
			return;

		base.OnCollectionChanged(e);
	}
}

public interface IFastObservableCollection
{
	public void Replace(IEnumerable other);
}