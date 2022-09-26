// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

using System;
using System.Collections;
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
using XivToolsWpf.Extensions;

/// <summary>
/// Interaction logic for SelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class Selector : UserControl, IFilterable, INotifyPropertyChanged
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
	public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(FilterBase), typeof(Selector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFilterChangedStatic)));

	private static readonly Dictionary<Type, string?> SearchInputs = new Dictionary<Type, string?>();
	private static readonly Dictionary<Type, double> ScrollPositions = new Dictionary<Type, double>();
	private readonly List<object> entries = new List<object>();

	public Selector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.ProgressBar.Visibility = Visibility.Visible;
	}

	public delegate void SelectorSelectedEvent(bool close);
	public delegate Task GetItemsEvent();

	public event PropertyChangedEventHandler? PropertyChanged;
	public event SelectorSelectedEvent? SelectionChanged;
	public event GetItemsEvent? LoadItems;

	public FastObservableCollection<object> FilteredItems { get; set; } = new ();

	public bool SearchEnabled { get; set; } = true;
	public bool HasSearch { get; set; } = false;
	public Type? ObjectType { get; set; }

	public IEnumerable<object> Entries => this.entries;

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
			try
			{
				if (!this.IsLoaded)
					return null;

				Decorator? border = VisualTreeHelper.GetChild(this.ListBox, 0) as Decorator;
				if (border == null)
					return null;

				return border.Child as ScrollViewer;
			}
			catch (Exception)
			{
				////Log.Error(ex, "Failed to get scrollviewer in selector");
				return null;
			}
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
			this.entries.Add(item);

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
				this.entries.Add(item);

				if (this.ObjectType == null)
				{
					this.ObjectType = item.GetType();
				}
			}
		}
	}

	public void FilterItems()
	{
		this.Filter?.Run();
	}

	public Task<IEnumerable<object>> GetAllItems() => Task.FromResult<IEnumerable<object>>(this.entries);

	public async Task SetFilteredItems(IEnumerable items)
	{
		await this.Dispatcher.MainThread();
		this.FilteredItems.Replace(items);
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
			newFilter.Filterable = selector;
		}
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

		if (this.LoadItems != null)
		{
			Task.Run(async () =>
			{
				await this.Dispatcher.MainThread();
				this.ClearItems();

				FilterBase? filter = this.Filter;

				await Dispatch.NonUiThread();
				await this.LoadItems.Invoke();

				if (filter != null)
					await filter.RunAsync();

				await this.Dispatcher.MainThread();
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

	private void OnClearSearchClicked(object sender, RoutedEventArgs e)
	{
		this.SearchBox.Text = string.Empty;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count <= 0)
			return;

		this.RaiseSelectionChanged();
	}

	private void OnDoubleClick(object sender, MouseButtonEventArgs e)
	{
		Point pos = e.GetPosition(this.ListBox);

		// over scrollbar
		if (pos.X > this.ListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth)
			return;

		this.SelectionChanged?.Invoke(true);
	}
}