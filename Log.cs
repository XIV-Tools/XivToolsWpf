// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Logging;

using System;

public static class Log
{
	public static Action<string>? HandleMessage;
	public static Action<Exception, string>? HandleError;

	public static void Message(string message) => HandleMessage?.Invoke(message);
	public static void Error(Exception ex, string message) => HandleError?.Invoke(ex, message);
}
