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

	public void AddRange(IEnumerable<T> other)
	{
		this.suppressChangedEvent = true;

		foreach (T item in other)
		{
			this.Add(item);
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