// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using PropertyChanged;
using Serilog;
using XivToolsWpf.Utils;

[AddINotifyPropertyChangedInterface]
public abstract class FilterBase : IComparer<object>, INotifyPropertyChanged
{
	protected bool abort = false;

	private readonly FuncQueue filterQueue;
	private string? search;

	public FilterBase()
	{
		this.filterQueue = new(this.Filter, 250);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string[]? SearchQuery { get; private set; }
	public IFilterable? Filterable { get; set; }

	public int FilterDelay
	{
		get => this.filterQueue.Delay;
		set => this.filterQueue.Delay = value;
	}

	public string? Search
	{
		get => this.search;
		set
		{
			this.search = value;

			if (string.IsNullOrEmpty(value))
			{
				this.SearchQuery = null;
			}
			else
			{
				this.SearchQuery = value.ToLower().Split(' ');
			}
		}
	}

	// Status
	public bool IsFiltering { get; protected set; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public void Run()
	{
		this.filterQueue.Invoke();
	}

	public async Task RunAsync()
	{
		this.filterQueue.Invoke();
		await this.filterQueue.WaitForPendingExecute();
	}

	public int Compare(object? x, object? y)
	{
		if (x == null || y == null)
			return 0;

		return this.CompareItems(x, y);
	}

	public abstract bool FilterItem(object obj);
	public abstract int CompareItems(object a, object b);

	protected virtual void OnPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
	{
		this.PropertyChanged?.Invoke(this, eventArgs);

		if (eventArgs.PropertyName == nameof(FilterBase.IsFiltering))
			return;

		// TODO: abort / queue / restart
		this.filterQueue.Invoke();
	}

	protected abstract Task Filter();
}

public abstract class FilterBase<T> : MultiThreadedFilterBase
{
	public sealed override bool FilterItem(object obj)
	{
		if (obj is T tObj)
			return this.FilterItem(tObj);

		return false;
	}

	public sealed override int CompareItems(object a, object b)
	{
		if (a is T tA && b is T tB)
			return this.CompareItems(tA, tB);

		return 0;
	}

	public abstract bool FilterItem(T obj);
	public abstract int CompareItems(T a, T b);
}

public interface IFilterable
{
	public Task<IEnumerable<object>> GetAllItems();
	public Task SetFilteredItems(IEnumerable items);
}