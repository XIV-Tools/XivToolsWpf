// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Inspector;

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;
using PropertyChanged.SourceGenerator;

using Binder = XivToolsWpf.DependencyProperties.Binder;
using System.ComponentModel;
using System.Windows;

public partial class InspectorView : UserControl
{
	public static readonly IBind<object?> TargetDp = Binder.Register<object?, InspectorView>(nameof(Target), OnTargetChanged, BindMode.OneWay);

	public InspectorView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		OnTargetChanged(this, this.Target);
	}

	public ObservableCollection<Entry> Entries { get; set; } = new();

	public object? Target
	{
		get => TargetDp.Get(this);
		set => TargetDp.Set(this, value);
	}

	private static void OnTargetChanged(InspectorView sender, object? value)
	{
		sender.Entries.Clear();

		if (value == null)
			return;

		Type targetType = value.GetType();
		PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

		foreach (PropertyInfo property in properties)
		{
			sender.Entries.Add(new(value, property));
		}
	}

	public partial class Entry : INotifyPropertyChanged
	{
		[Notify] private object target;
		[Notify] private PropertyInfo property;

		public Entry(object target, PropertyInfo property)
		{
			this.property = property;
			this.target = target;

			if (this.Target is INotifyPropertyChanged changed)
			{
				changed.PropertyChanged += this.OnTargetPropertyChanged;
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public string Name => this.Property.Name;
		public Type Type => this.Property.PropertyType;

		public bool CanWrite => this.Property.CanWrite;

		public object? Value
		{
			get => this.Property.GetValue(this.Target);
			set
			{
				if (!this.CanWrite)
					return;

				this.Property.SetValue(this.Target, value);
			}
		}

		private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != this.Property.Name)
				return;

			this.PropertyChanged?.Invoke(this, new(nameof(this.Value)));
		}
	}
}

public class TemplateSelector : DataTemplateSelector
{
	public override DataTemplate SelectTemplate(object? item, DependencyObject container)
	{
		if (item is not InspectorView.Entry entry)
			return base.SelectTemplate(item, container);

		DataTemplate? drawer = Drawers.GetDrawer(entry.Type);

		if (drawer != null)
			return drawer;

		return base.SelectTemplate(item, container);
	}
}