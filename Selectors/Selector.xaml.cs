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
using System.Windows.Media;
using PropertyChanged;
using Serilog;
using XivToolsWpf;

/// <summary>
/// Interaction logic for SelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class Selector : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(FilterBase), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFilterChangedStatic)));

	private static readonly Dictionary<Type, string?> SearchInputs = new Dictionary<Type, string?>();
	private static readonly Dictionary<Type, double> ScrollPositions = new Dictionary<Type, double>();
	private readonly List<ItemEntry> entries = new List<ItemEntry>();

	private bool searching = false;
	private bool idle = true;
	private string[]? searchQuery;
	private bool xamlLoading = false;
	private bool abortSearch = false;
	private bool isFiltering = false;

	public Selector()
	{
		this.InitializeComponent();
		this.xamlLoading = true;
		this.ContentArea.DataContext = this;

		this.PropertyChanged += this.OnPropertyChanged;
		this.ProgressBar.Visibility = Visibility.Visible;
	}

	public delegate void SelectorSelectedEvent(bool close);
	public delegate Task GetItemsEvent();

	public event PropertyChangedEventHandler? PropertyChanged;
	public event SelectorSelectedEvent? SelectionChanged;
	public event GetItemsEvent? LoadItems;

	public ObservableCollection<object> FilteredItems { get; set; } = new ObservableCollection<object>();

	public bool SearchEnabled { get; set; } = true;
	public bool HasSearch { get; set; } = false;
	public Type? ObjectType { get; set; }

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

	public FilterBase? Filter
	{
		get => (FilterBase)this.GetValue(FilterProperty);
		set => this.SetValue(FilterProperty, value);
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

	public double ScrollPosition
	{
		get
		{
			ScrollViewer? scroll = this.ScrollViewer;
			if (scroll == null)
				return 0;

			return scroll.VerticalOffset;
		}

		set
		{
			ScrollViewer? scroll = this.ScrollViewer;
			if (scroll == null)
				return;

			scroll.ScrollToVerticalOffset(value);
		}
	}

	private static ILogger Log => Serilog.Log.ForContext<Selector>();

	private ScrollViewer? ScrollViewer
	{
		get
		{
			Decorator? border = VisualTreeHelper.GetChild(this.ListBox, 0) as Decorator;
			if (border == null)
				return null;

			return border.Child as ScrollViewer;
		}
	}

	public void OnClosed()
	{
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

			if (this.ObjectType == null)
			{
				this.ObjectType = item.GetType();
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

				if (this.ObjectType == null)
				{
					this.ObjectType = item.GetType();
				}
			}
		}
	}

	public void FilterItems()
	{
		Task.Run(this.DoFilter);
	}

	public Task FilterItemsAsync()
	{
		return this.DoFilter();
	}

	public void RaiseSelectionChanged()
	{
		this.SelectionChanged?.Invoke(false);
	}

	private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is Selector view)
		{
			view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
		}
	}

	private static void OnFilterChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is Selector selector && e.NewValue is FilterBase newFilter)
		{
			if (e.OldValue is FilterBase oldFilter)
				oldFilter.PropertyChanged -= selector.OnFilterChanged;

			newFilter.PropertyChanged += selector.OnFilterChanged;
		}
	}

	private void OnFilterChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.isFiltering)
			return;

		this.FilterItems();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.ObjectType != null)
		{
			if (SearchInputs.ContainsKey(this.ObjectType))
				this.SearchBox.Text = SearchInputs[this.ObjectType];

			if (ScrollPositions.ContainsKey(this.ObjectType))
			{
				this.ScrollPosition = ScrollPositions[this.ObjectType];
			}
		}

		Keyboard.Focus(this.SearchBox);
		this.SearchBox.CaretIndex = int.MaxValue;
		this.xamlLoading = false;

		if (this.LoadItems != null)
		{
			Task.Run(async () =>
			{
				await Dispatch.MainThread();
				this.ClearItems();

				await Dispatch.NonUiThread();
				await this.LoadItems.Invoke();

				await this.FilterItemsAsync();

				await Dispatch.MainThread();
				this.ProgressBar.Visibility = Visibility.Collapsed;

				this.ListBox.ScrollIntoView(this.Value);
			});
		}
		else
		{
			this.ProgressBar.Visibility = Visibility.Collapsed;
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (this.ObjectType == null)
			return;

		if (!SearchInputs.ContainsKey(this.ObjectType))
			SearchInputs.Add(this.ObjectType, null);

		SearchInputs[this.ObjectType] = this.SearchBox.Text;

		if (!ScrollPositions.ContainsKey(this.ObjectType))
			ScrollPositions.Add(this.ObjectType, 0);

		ScrollPositions[this.ObjectType] = this.ScrollPosition;
	}

	private void OnSearchChanged(object sender, TextChangedEventArgs e)
	{
		if (this.ObjectType == null)
			return;

		string str = this.SearchBox.Text;

		this.HasSearch = !string.IsNullOrWhiteSpace(str);

		SearchInputs[this.ObjectType] = str;
		Task.Run(async () => { await this.Search(str); });

		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HasSearch)));
	}

	private void OnClearSearchClicked(object sender, RoutedEventArgs e)
	{
		this.SearchBox.Text = string.Empty;
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

		if (!this.xamlLoading)
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
				this.searchQuery = null;
			}
			else
			{
				str = str.ToLower();
				this.searchQuery = str.Split(' ');
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
		while (this.isFiltering)
			await Task.Delay(100);

		this.idle = false;
		this.isFiltering = true;

		if (!this.SearchEnabled)
			this.searchQuery = null;

		await Dispatch.MainThread();

		ConcurrentQueue<ItemEntry> entries;
		lock (this.entries)
		{
			entries = new ConcurrentQueue<ItemEntry>(this.entries);
		}

		FilterBase? filter = this.Filter;

		await Dispatch.NonUiThread();

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
						if (filter != null && !filter.FilterItem(entry.Item, this.searchQuery))
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

		if (filter != null)
		{
			sortedFilteredEntries = sortedFilteredEntries.OrderBy(cc => cc.Item, filter);
		}

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
		this.isFiltering = false;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count <= 0)
			return;

		if (this.searching)
			return;

		this.RaiseSelectionChanged();
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
		Point pos = e.GetPosition(this.ListBox);

		// over scrollbar
		if (pos.X > this.ListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth)
			return;

		this.SelectionChanged?.Invoke(true);
	}

	private struct ItemEntry
	{
		public object Item;
		public int OriginalIndex;
	}

	[AddINotifyPropertyChangedInterface]
	public abstract class FilterBase : IComparer<object>, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public int Compare(object? x, object? y)
		{
			if (x == null || y == null)
				return 0;

			return this.CompareItems(x, y);
		}

		public abstract bool FilterItem(object obj, string[]? search);
		public abstract int CompareItems(object a, object b);
	}

	public abstract class FilterBase<T> : FilterBase
	{
		public sealed override bool FilterItem(object obj, string[]? search)
		{
			if (obj is T tObj)
				return this.FilterItem(tObj, search);

			return false;
		}

		public sealed override int CompareItems(object a, object b)
		{
			if (a is T tA && b is T tB)
				return this.CompareItems(tA, tB);

			return 0;
		}

		public abstract bool FilterItem(T obj, string[]? search);
		public abstract int CompareItems(T a, T b);
	}
}
