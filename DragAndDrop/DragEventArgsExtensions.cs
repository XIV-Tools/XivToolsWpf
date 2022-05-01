// © XIV-Tools.
// Licensed under the MIT license.

namespace System.Windows;

public static class DragEventArgsExtensions
{
	public static void SetContext(this DataObject self, object context)
	{
		self.SetData("Context", context);
	}

	public static T? GetContext<T>(this DragEventArgs self)
		where T : notnull
	{
		if (self.GetContext() is T tVal)
			return tVal;

		return default;
	}

	public static object? GetContext(this DragEventArgs self)
	{
		return self.Data.GetData("Context");
	}

	public static void SetSource(this DataObject self, FrameworkElement source)
	{
		self.SetData("Source", source);
	}

	public static void SetSource(this DragEventArgs self, FrameworkElement source)
	{
		self.Data.SetData("Source", source);
	}

	public static FrameworkElement GetSource(this DragEventArgs self)
	{
		return self.GetData<FrameworkElement>("Source");
	}

	public static void SetTarget(this DragEventArgs self, FrameworkElement target)
	{
		self.Data.SetData("Target", target);
	}

	public static FrameworkElement GetTarget(this DragEventArgs self)
	{
		return self.GetData<FrameworkElement>("Target");
	}

	public static void SetData(this DragEventArgs self, string name, object value)
	{
		self.Data.SetData(name, value);
	}

	public static T GetData<T>(this DragEventArgs self, string name)
		where T : notnull
	{
		if (self.Data.GetData(name) is T tVal)
			return tVal;

		throw new Exception($"Data {name} not found");
	}
}
