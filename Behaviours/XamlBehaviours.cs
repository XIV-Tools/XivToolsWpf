// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Behaviours;

using System;
using System.Runtime.CompilerServices;
using System.Windows;

public static class XamlBehaviours
{
	private static readonly ConditionalWeakTable<FrameworkElement, Behaviour> AttachedHandlers = new();

	public static void AttachHandler<T>(this FrameworkElement element, bool enable, object? value = null)
		where T : Behaviour
	{
		if (enable)
		{
			if (!AttachedHandlers.TryGetValue(element, out var handler))
			{
				if (value == null)
				{
					handler = Activator.CreateInstance(typeof(T), new[] { element }) as Behaviour;
				}
				else
				{
					handler = Activator.CreateInstance(typeof(T), new[] { element, value }) as Behaviour;
				}

				if (handler == null)
					throw new InvalidOperationException();

				AttachedHandlers.Add(element, handler);
			}
		}
		else
		{
			if (!AttachedHandlers.TryGetValue(element, out var handler))
				return;

			handler.Dispose();
			AttachedHandlers.Remove(element);
		}
	}
}