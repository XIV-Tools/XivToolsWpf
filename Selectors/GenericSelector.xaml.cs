// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using XivToolsWpf;

using static XivToolsWpf.Selectors.Selector;

/// <summary>
/// Interaction logic for GenericSelector.xaml.
/// </summary>
public partial class GenericSelector : UserControl
{
	public GenericSelector(IEnumerable<ISelectable> options)
	{
		this.InitializeComponent();

		this.Selector.AddItems(options);
		this.Selector.FilterItems();
	}

	public event SelectorSelectedEvent? SelectionChanged;

	public ISelectable? Value
	{
		get
		{
			object? val = this.Selector.Value;

			if (val is ISelectable itemVal)
				return itemVal;

			return null;
		}

		set
		{
			this.Selector.Value = value;
		}
	}

	private void OnSelectionChanged(bool close)
	{
		this.SelectionChanged?.Invoke(close);
	}

#pragma warning disable SA1011
	private bool OnFilter(object obj, string[]? search = null)
	{
		if (obj is ISelectable item)
		{
			if (string.IsNullOrEmpty(item.Name))
				return false;

			if (!SearchUtility.Matches(item.Name, search))
				return false;

			return true;
		}

		return false;
	}
}

#pragma warning disable SA1201
public interface ISelectable
{
	string Name { get; }
	string? Description { get; }
}
