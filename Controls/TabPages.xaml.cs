// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using FontAwesome.Sharp;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

public partial class TabPages : UserControl
{
	public TabPages()
	{
		this.InitializeComponent();
		this.Tabs.CollectionChanged += this.OnTabsChanged;
	}

	public ObservableCollection<TabPage> Tabs { get; private set; } = new();
	public ObservableCollection<TabPage> Pages { get; private set; } = new();

	public void AddPage<T>(string name, IconChar icon)
		where T : UserControl
	{
		TabPage<T> page = new TabPage<T>(icon, name);
		this.Tabs.Add(page);
		this.Pages.Add(page);
	}

	private TabPage GetPage(string name)
	{
		foreach (TabPage page in this.Tabs)
		{
			if (page.Name == name)
			{
				return page;
			}
		}

		throw new Exception($"No page found with name: {name}");
	}

	private void OnTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
	}

	private void OnTabSelected(object sender, RoutedEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		foreach (TabPage page in this.Pages)
		{
			page.IsActive = senderElement.DataContext == page;
		}
	}
}

[AddINotifyPropertyChangedInterface]
public abstract class TabPage
{
	private UserControl? control;

	public TabPage(IconChar icon, string name)
	{
		this.Icon = icon;
		this.Name = name;
		this.DisplayNameKey = name;
		this.TooltipKey = $"{name}_Tooltip";
	}

	public string Name { get; private set; }
	public int Index { get; set; }
	public string DisplayNameKey { get; private set; }
	public string TooltipKey { get; private set; }

	[DependsOn(nameof(TabPage.IsActive))]
	public UserControl? Content
	{
		get
		{
			if (this.control == null)
			{
				if (!this.IsActive)
					return null;

				this.control = this.CreateContent();
			}

			return this.control;
		}
	}

	public IconChar Icon { get; private set; }
	public bool IsActive { get; set; }
	public object? DataContext { get; set; }

	protected abstract UserControl CreateContent();
}

public class TabPage<T> : TabPage
	where T : UserControl
{
	public TabPage(IconChar icon, string name)
		: base(icon, name)
	{
	}

	protected override UserControl CreateContent()
	{
		UserControl? control = Activator.CreateInstance<T>();

		if (control == null)
			throw new Exception($"Failed to create page content: {typeof(T)}");

		return control;
	}
}