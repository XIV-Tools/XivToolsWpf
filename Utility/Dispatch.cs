// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using System.Windows;

	public static class Dispatch
	{
		public static SwitchToUiAwaitable MainThread()
		{
			return default(SwitchToUiAwaitable);
		}

		public static SwitchFromUiAwaitable NonUiThread()
		{
			return default(SwitchFromUiAwaitable);
		}

		public struct SwitchToUiAwaitable : INotifyCompletion
		{
			public bool IsCompleted => Application.Current?.Dispatcher.CheckAccess() == false;

			public SwitchToUiAwaitable GetAwaiter()
			{
				return this;
			}

			public void GetResult()
			{
			}

			public void OnCompleted(Action continuation)
			{
				Application.Current?.Dispatcher.BeginInvoke(continuation);
			}
		}

		public struct SwitchFromUiAwaitable : INotifyCompletion
		{
			public bool IsCompleted => Application.Current?.Dispatcher.CheckAccess() == false;

			public SwitchFromUiAwaitable GetAwaiter()
			{
				return this;
			}

			public void GetResult()
			{
			}

			public void OnCompleted(Action continuation)
			{
				Task.Run(continuation);
			}
		}
	}
}
