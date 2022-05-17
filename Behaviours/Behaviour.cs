// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Behaviours;

using System;
using System.Windows;

public abstract class Behaviour : IDisposable
{
	public readonly DependencyObject Host;

	public Behaviour(DependencyObject host)
	{
		this.Host = host;

		if (host is FrameworkElement frameworkElement)
		{
			frameworkElement.Loaded += this.OnSelfLoaded;
		}
		else if (host is FrameworkContentElement frameworkContentElement)
		{
			frameworkContentElement.Loaded += this.OnSelfLoaded;
		}
	}

	public Behaviour(DependencyObject el, object? value)
		: this(el)
	{
	}

	public virtual void OnLoaded()
	{
	}

	public virtual void Dispose()
	{
	}

	private void OnSelfLoaded(object sender, RoutedEventArgs e)
	{
		this.OnLoaded();
	}
}