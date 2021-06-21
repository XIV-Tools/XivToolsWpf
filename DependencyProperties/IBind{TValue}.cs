// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyProperties
{
	using System.Windows;

	public interface IBind<TValue>
	{
		TValue Get(DependencyObject control);
		void Set(DependencyObject control, TValue value);
	}
}
