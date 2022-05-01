// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Behaviours;

using System;
using System.Windows;

public abstract class Behaviour : IDisposable
{
	public Behaviour(FrameworkElement el)
	{
	}

	public virtual void Dispose()
	{
	}
}