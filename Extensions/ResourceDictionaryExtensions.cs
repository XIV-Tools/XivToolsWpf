// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Extensions;

using System;
using System.Windows;

public static class ResourceDictionaryExtensions
{
	public static void Set(this ResourceDictionary self, string key, object value)
	{
		if (!self.Contains(key))
		{
			self.Add(key, value);
		}
		else
		{
			self[key] = value;
		}
	}
}
