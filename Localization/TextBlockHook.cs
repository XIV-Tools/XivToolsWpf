// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Localization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

public static class TextBlockHook
{
	public static Func<string, string?>? GetLocalizedText;

	private static readonly ConditionalWeakTable<TextBlock, string> FormattedTextBlocks = new();
	private static PropertyChangedCallback? originalCallback;
	private static bool isAttached = false;

	// Get the Flags we need
	// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/TextBlock.cs,3871
	private enum Flags
	{
		TextContentChanging = 0x400,
	}

	public static void UpdateAll()
	{
		foreach(KeyValuePair<TextBlock, string> entry in FormattedTextBlocks)
		{
			entry.Key.SetFormattedText(entry.Value);
		}
	}

	public static void Attach()
	{
		if (isAttached)
			return;

		isAttached = true;

		// Get the internal property changed callback for '.Text' and replace it with our own.
		// https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/PropertyMetadata.cs,682
		FieldInfo? field = typeof(PropertyMetadata).GetField("_propertyChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance);
		if (field == null)
			throw new Exception("Failed to get _propertyChangedCallback field from PropertyMetadata");

		PropertyMetadata metaData = TextBlock.TextProperty.GetMetadata(typeof(TextBlock));
		originalCallback = field.GetValue(metaData) as PropertyChangedCallback;
		field.SetValue(metaData, new PropertyChangedCallback(OnTextChanged));
	}

	// Use reflection to call into the private SetFlags method
	// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/TextBlock.cs,3548
	private static void SetFlags(this TextBlock textBlock, bool enable, Flags flag)
	{
		MethodInfo? method = typeof(TextBlock).GetMethod("SetFlags", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

		if (method == null)
			throw new Exception("Failed to get SetFlags method from TextBlock");

		method.Invoke(textBlock, new object[] { enable, flag });
	}

	private static void OnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
	{
		originalCallback?.Invoke(dependencyObject, args);

		try
		{
			TextBlock textBlock = (TextBlock)dependencyObject;
			string newString = (string)args.NewValue;
			string oldString = (string)args.OldValue;

			if (newString == oldString)
				return;

			if (string.IsNullOrEmpty(newString))
				return;

			if (newString.StartsWith('['))
			{
				newString = newString.Trim('[', ']');
				textBlock.SetFormattedText(newString);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error in applying formatted text: {ex.Message}");
			throw;
		}
	}

	private static void SetFormattedText(this TextBlock textBlock, string input)
	{
		string? text = null;
		if (GetLocalizedText != null)
		{
			text = GetLocalizedText(input);
		}
		else
		{
			text = "[?" + input + "?]";
		}

		if (text == null)
			return;

		BindingExpression? oldBinding = textBlock.GetBindingExpression(TextBlock.TextProperty);

		FormattedTextBlocks.AddOrUpdate(textBlock, input);
		textBlock.SetFlags(true, Flags.TextContentChanging);
		textBlock.Inlines.Clear();
		textBlock.Inlines.Add(new Run(text));
		textBlock.SetFlags(false, Flags.TextContentChanging);

		// Sanity check that we havn't lost our text binding
		BindingExpression? newBinding = textBlock.GetBindingExpression(TextBlock.TextProperty);
		if (oldBinding != newBinding)
		{
			throw new Exception("Updating text format cleared binding");
		}
	}
}