﻿// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Inspector;

using System;
using System.Collections;
using System.Windows;

public static class Drawers
{
	private static ResourceDictionary? resourceDictionary;

	private static DataTemplate? genericDrawer;

	public static DataTemplate? GetDrawer(Type objectType)
	{
		if (genericDrawer == null)
		{
			genericDrawer = FindDrawer(typeof(void));
		}

		// TODO:Cache these for faster lookups.
		DataTemplate? template = FindDrawer(objectType);
		if (template != null)
			return template;

		return genericDrawer;
	}

	public static DataTemplate? FindDrawer(Type objectType)
	{
		if (resourceDictionary == null)
		{
			resourceDictionary = new();
			resourceDictionary.Source = new Uri("XivToolsWpf;component/Inspector/Drawers.xaml", UriKind.RelativeOrAbsolute);
		}

		foreach (object? item in resourceDictionary)
		{
			if (item is DictionaryEntry entry &&
				entry.Value is DataTemplate template &&
				template.DataType is Type targetType &&
				targetType == objectType)
			{
				return template;
			}
		}

		return null;
	}
}
