// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class FlagCheckbox : UserControl
{
	public static readonly IBind<string?> LabelDp = Binder.Register<string?, FlagCheckbox>(nameof(Label));
	public static readonly IBind<string?> FlagDp = Binder.Register<string?, FlagCheckbox>(nameof(Flag));
	public static readonly IBind<Enum?> ValueDp = Binder.Register<Enum?, FlagCheckbox>(nameof(Value), OnValueChanged);

	public FlagCheckbox()
	{
		this.InitializeComponent();
		this.Checkbox.DataContext = this;
	}

	public string? Label
	{
		get => LabelDp.Get(this);
		set => LabelDp.Set(this, value);
	}

	public Enum? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public string? Flag
	{
		get => FlagDp.Get(this);
		set => FlagDp.Set(this, value);
	}

	private static void OnValueChanged(FlagCheckbox sender, Enum? value)
	{
		if (value == null || sender.Flag == null || sender.Value == null)
			return;

		Type enumType = sender.Value.GetType();
		Enum flagValue = (Enum)Enum.Parse(enumType, sender.Flag);

		sender.Checkbox.IsChecked = value.HasFlag(flagValue);
	}

	private void OnChecked(object sender, RoutedEventArgs e)
	{
		if (this.Value == null || this.Flag == null)
			return;

		Type enumType = this.Value.GetType();
		Enum value = (Enum)Enum.Parse(enumType, this.Flag);

		if (Enum.GetUnderlyingType(enumType) != typeof(ulong))
		{
			this.Value = (Enum)Enum.ToObject(enumType, Convert.ToInt64(this.Value) | Convert.ToInt64(value));
		}
		else
		{
			this.Value = (Enum)Enum.ToObject(enumType, Convert.ToUInt64(this.Value) | Convert.ToUInt64(value));
		}
	}

	private void OnUnchecked(object sender, RoutedEventArgs e)
	{
		if (this.Value == null || this.Flag == null)
			return;

		Type enumType = this.Value.GetType();
		Enum value = (Enum)Enum.Parse(enumType, this.Flag);

		if (Enum.GetUnderlyingType(enumType) != typeof(ulong))
		{
			this.Value = (Enum)Enum.ToObject(enumType, Convert.ToInt64(this.Value) & ~Convert.ToInt64(value));
		}
		else
		{
			this.Value = (Enum)Enum.ToObject(enumType, Convert.ToUInt64(this.Value) & ~Convert.ToUInt64(value));
		}
	}
}
