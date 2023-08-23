// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using XivToolsWpf.Logging;

/// <summary>
/// A multithreaded implementation of filter that uses multiple concurrent tasks
/// to process items quickly.
/// </summary>
public abstract class MultiThreadedFilterBase : FilterBase
{
	public int ThreadCount = 1;

	protected override async Task<IEnumerable<object>?> Filter()
	{
		if (this.Filterable == null)
			return null;

		IFilterable filterable = this.Filterable;
		ConcurrentQueue<object> itemsToFilter = new(await filterable.GetAllItems());

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
						Log.Error(ex, $"Failed to filter item: {item}");
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
		return filteredEntries;
	}
}
