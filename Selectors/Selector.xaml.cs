// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;

/// <summary>
/// Interaction logic for SelectorDrawer.xaml.
/// </summary>
public partial class Selector : UserControl, ISelector, INotifyPropertyChanged
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

	private static readonly Dictionary<Type, string?> SearchInputs = new Dictionary<Type, string?>();
	private readonly List<ItemEntry> entries = new List<ItemEntry>();

	private Type? objectType;
	private bool searching = false;
	private bool idle = true;
	private string[]? searchQuerry;
	private bool loading = false;
	private bool abortSearch = false;

	public Selector()
	{
		this.InitializeComponent();
		this.loading = true;
		this.DataContext = this;

		this.PropertyChanged += this.OnPropertyChanged;
	}

	public delegate bool FilterEvent(object item, string[]? search);

	public event FilterEvent? Filter;
	public event SelectorSelectedEvent? SelectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<object> FilteredItems { get; set; } = new ObservableCollection<object>();

	public IEnumerable<object> Entries
	{
		get
		{
			List<object> values = new List<object>();
			foreach (ItemEntry entry in this.entries)
				values.Add(entry.Item);

			return values;
		}
	}

	public object? Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	public DataTemplate ItemTemplate
	{
		get => (DataTemplate)this.GetValue(ItemTemplateProperty);
		set => this.SetValue(ItemTemplateProperty, value);
	}

	private static ILogger Log => Serilog.Log.ForContext<Selector>();

	public static void Show<TView, TValue>(TValue? current, Action<TValue, bool> changed)
		where TView : ISelectorView
		where TValue : class
	{
		ISelectorView view = Activator.CreateInstance<TView>();
		Show(view, current, changed);
	}

	public static void Show<TValue>(ISelectorView view, TValue? current, Action<TValue, bool> changed)
		where TValue : class
	{
		view.Selector.objectType = typeof(TValue);
		view.Selector.Value = current;
		view.Selector.SelectionChanged += (bool close) =>
		{
			object? v = view.Selector.Value;
			if (v is TValue tval)
			{
				changed?.Invoke(tval, close);
			}
		};

		throw new NotImplementedException();
	}

	public void ClearItems()
	{
		lock (this.entries)
		{
			this.entries.Clear();
		}
	}

	public void AddItem(object item)
	{
		lock (this.entries)
		{
			ItemEntry entry = default;
			entry.Item = item;
			entry.OriginalIndex = this.entries.Count;
			this.entries.Add(entry);

			if (this.objectType == null)
			{
				this.objectType = item.GetType();
			}
		}
	}

	public void AddItems(IEnumerable<object> items)
	{
		lock (this.entries)
		{
			foreach (object item in items)
			{
				ItemEntry entry = default;
				entry.Item = item;
				entry.OriginalIndex = this.entries.Count;
				this.entries.Add(entry);

				if (this.objectType == null)
				{
					this.objectType = item.GetType();
				}
			}
		}
	}

	public void FilterItems()
	{
		Task.Run(this.DoFilter);
	}

	private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is Selector view)
		{
			view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.objectType != null && SearchInputs.ContainsKey(this.objectType))
			this.SearchBox.Text = SearchInputs[this.objectType];

		Keyboard.Focus(this.SearchBox);
		this.SearchBox.CaretIndex = int.MaxValue;
		this.loading = false;
	}

	private void OnSearchChanged(object sender, TextChangedEventArgs e)
	{
		if (this.objectType == null)
			return;

		if (!SearchInputs.ContainsKey(this.objectType))
			SearchInputs.Add(this.objectType, null);

		string str = this.SearchBox.Text;
		SearchInputs[this.objectType] = str;
		Task.Run(async () => { await this.Search(str); });
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.entries))
		{
			Task.Run(this.DoFilter);
		}
	}

	private async Task Search(string str)
	{
		this.idle = false;
		this.abortSearch = true;

		if (!this.loading)
			await Task.Delay(50);

		try
		{
			while (this.searching)
				await Task.Delay(100);

			this.searching = true;
			string currentInput = await Application.Current.Dispatcher.InvokeAsync<string>(() =>
			{
				return this.SearchBox.Text;
			});

			// If the input was changed, abort this task
			if (str != currentInput)
			{
				this.searching = false;
				return;
			}

			if (string.IsNullOrEmpty(str))
			{
				this.searchQuerry = null;
			}
			else
			{
				str = str.ToLower();
				this.searchQuerry = str.Split(' ');
			}

			this.abortSearch = false;
			await Task.Run(this.DoFilter);
			this.searching = false;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to perform search");
		}

		this.idle = true;
	}

	private async Task DoFilter()
	{
		this.idle = false;

		ConcurrentQueue<ItemEntry> entries;
		lock (this.entries)
		{
			entries = new ConcurrentQueue<ItemEntry>(this.entries);
		}

		ConcurrentBag<ItemEntry> filteredEntries = new ConcurrentBag<ItemEntry>();

		int threads = 4;
		List<Task> tasks = new List<Task>();
		for (int i = 0; i < threads; i++)
		{
			Task t = Task.Run(() =>
			{
				while (!entries.IsEmpty)
				{
					ItemEntry entry;
					if (!entries.TryDequeue(out entry))
						continue;

					try
					{
						if (this.Filter != null && !this.Filter.Invoke(entry.Item, this.searchQuerry))
							continue;
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"Failed to filter selector item: {entry.Item}");
					}

					filteredEntries.Add(entry);

					if (this.abortSearch)
					{
						entries.Clear();
					}
				}
			});

			tasks.Add(t);
		}

		await Task.WhenAll(tasks.ToArray());

		IOrderedEnumerable<ItemEntry>? sortedFilteredEntries = filteredEntries.OrderBy(cc => cc.OriginalIndex);

		await Application.Current.Dispatcher.InvokeAsync(() =>
		{
			this.FilteredItems.Clear();

			foreach (ItemEntry obj in sortedFilteredEntries)
			{
				this.FilteredItems.Add(obj.Item);
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FilteredItems)));
		});

		this.idle = true;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count <= 0)
			return;

		if (this.searching)
			return;

		this.SelectionChanged?.Invoke(false);
	}

	private async void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		while (!this.idle)
			await Task.Delay(10);

		if (this.FilteredItems.Count <= 0)
			return;

		this.Value = this.FilteredItems[0];
	}

	private void OnDoubleClick(object sender, MouseButtonEventArgs e)
	{
		this.SelectionChanged?.Invoke(true);
	}

	private struct ItemEntry
	{
		public object Item;
		public int OriginalIndex;
	}
}
