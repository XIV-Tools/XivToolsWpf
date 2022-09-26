// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections;

/// <summary>
/// A multithreaded implementation of filter that uses multiple concurrent tasks
/// to process items quickly.
/// </summary>
public abstract class MultiThreadedFilterBase : FilterBase
{
	public int ThreadCount = 1;

	protected override async Task Filter()
	{
		await Dispatch.NonUiThread();

		while (this.IsFiltering)
			await Task.Delay(100);

		if (this.Filterable == null)
			return;

		this.IsFiltering = true;

		try
		{
			IFilterable filterable = this.Filterable;
			IEnumerable<object> allItems = await filterable.GetAllItems();

			ConcurrentQueue<object> itemsToFilter;
			lock (filterable)
			{
				itemsToFilter = new(allItems);
			}

			ConcurrentBag<object> filteredEntries = new();

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < this.ThreadCount; i++)
			{
				Task t = Task.Run(() =>
				{
					while (!itemsToFilter.IsEmpty)
					{
						object? item;
						if (!itemsToFilter.TryDequeue(out item) || item == null)
							continue;

						try
						{
							if (!this.FilterItem(item))
							{
								continue;
							}
						}
						catch (Exception ex)
						{
							this.Log.Error(ex, $"Failed to filter item: {item}");
						}

						filteredEntries.Add(item);

						if (this.abort)
						{
							itemsToFilter.Clear();
						}
					}
				});

				tasks.Add(t);
			}

			await Task.WhenAll(tasks.ToArray());

			IOrderedEnumerable<object>? sortedFilteredEntries = filteredEntries.OrderBy(cc => cc, this);

			await filterable.SetFilteredItems(sortedFilteredEntries);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error running filter");
		}

		this.IsFiltering = false;
	}
}
