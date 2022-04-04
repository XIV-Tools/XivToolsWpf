// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Windows
{
	using System;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Interop;

	public static class WindowExtensions
	{
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		private static bool GetIsActive(this Window? self)
		{
			if (self == null)
				return false;

			return GetForegroundWindow() == new WindowInteropHelper(self).Handle;
		}
	}
}
