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
			public bool IsCompleted
			{
				get
				{
					if (Application.Current == null)
						return true;

					return Application.Current.Dispatcher.CheckAccess();
				}
			}

			public SwitchToUiAwaitable GetAwaiter()
			{
				return this;
			}

			public void GetResult()
			{
			}

			public void OnCompleted(Action continuation)
			{
				if (Application.Current == null)
					return;

				Application.Current.Dispatcher.BeginInvoke(continuation);
			}
		}

		public struct SwitchFromUiAwaitable : INotifyCompletion
		{
			public bool IsCompleted => !Application.Current.Dispatcher.CheckAccess();

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
