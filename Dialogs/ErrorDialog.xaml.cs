// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Dialogs;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Serilog;
using XivToolsWpf.Windows;

/// <summary>
/// Interaction logic for ErrorDialog.xaml.
/// </summary>
public partial class ErrorDialog : UserControl
{
	private Window? window;

	public ErrorDialog(Window? host, ExceptionDispatchInfo exDispatch, bool isCritical)
	{
		this.InitializeComponent();

		this.window = host;

		this.OkButton.Visibility = isCritical ? Visibility.Collapsed : Visibility.Visible;
		this.QuitButton.Visibility = !isCritical ? Visibility.Collapsed : Visibility.Visible;

		this.Message.Text = isCritical ? "Critical Error" : "Error";
		this.Subtitle.Visibility = isCritical ? Visibility.Visible : Visibility.Collapsed;

		Exception? ex = exDispatch.SourceException;
		if (exDispatch.SourceException is AggregateException aggEx && aggEx.InnerException != null)
			ex = aggEx.InnerException;

		StringBuilder builder = new StringBuilder();
		builder.Append(ex.Message);

		if (ex.InnerException != null)
		{
			builder.AppendLine();
			builder.Append(ex.InnerException.Message);
		}

		this.HeaderTextBlock.Text = builder.ToString();

		builder.Clear();
		while (ex != null)
		{
			this.StackTraceBlock.Inlines.Add(new Run("[" + ex.GetType().Name + "] ") { FontWeight = FontWeights.Bold });
			this.StackTraceBlock.Inlines.Add(ex.Message);
			this.StackTraceBlock.Inlines.Add("\n");

			if (ex.StackTrace != null)
				this.StackTraceFormatter(ex.StackTrace);

			ex = ex.InnerException;
		}

		if (Debugger.IsAttached)
		{
			this.DetailsExpander.IsExpanded = true;
		}
	}

	public static void ShowError(ExceptionDispatchInfo ex, bool isCriticial)
	{
		if (Application.Current == null)
			return;

		if (ex.SourceException is ErrorException)
			return;

		Application.Current.Dispatcher.Invoke(() =>
		{
			try
			{
				ErrorDialog errorDialog = new ErrorDialog(null, ex, isCriticial);
				errorDialog.window = new StyledWindow(errorDialog);
				errorDialog.window.ShowDialog();

				if (Application.Current == null)
					return;

				if (isCriticial)
				{
					Application.Current.Shutdown(2);
				}
			}
			catch (Exception innerEx)
			{
				Log.Error(new ErrorException(innerEx), "Failed to display error dialog");
			}
		});
	}

	private void StackTraceFormatter(string stackTrace)
	{
		if (string.IsNullOrEmpty(stackTrace))
			return;

		string[] lines = stackTrace.Split('\n');
		foreach (string line in lines)
		{
			this.StackTraceLineFormatter(line);
		}
	}

	private void StackTraceLineFormatter(string line)
	{
		string[] parts = line.Split(new[] { " in " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length == 2)
		{
			this.StackTraceBlock.Inlines.Add(new Run(parts[0]));
			this.StackTraceBlock.Inlines.Add(new Run(" @ ") { Foreground = Brushes.LightGray });

			string? path;
			if (this.GetPath(parts[1], out path, out _) && File.Exists(path))
			{
				Hyperlink link = new Hyperlink(new Run(parts[1] + "\n"));
				link.RequestNavigate += this.Link_RequestNavigate;
				link.NavigateUri = new Uri(parts[1]);
				this.StackTraceBlock.Inlines.Add(link);
			}
			else
			{
				this.StackTraceBlock.Inlines.Add(new Run(parts[1] + "\n") { Foreground = Brushes.Gray });
			}
		}
		else
		{
			this.StackTraceBlock.Inlines.Add(parts[0]);
		}
	}

	private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		string? path;
		string? line;

		if (!this.GetPath(e.Uri.OriginalString, out path, out line))
			return;

		try
		{
			Process[] procs = Process.GetProcessesByName("devenv");
			if (procs.Length != 0)
			{
				string? devEnvPath = procs[0].MainModule?.FileName;

				if (devEnvPath == null)
					return;

				Process.Start(devEnvPath, $"-Edit \"{path}\" -Command \"Edit.Goto {line}\"");
			}
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to navigate to source file");
		}
	}

	private bool GetPath(string stackLine, out string? path, out string? line)
	{
		path = null;
		line = null;

		stackLine = stackLine.Trim();
		string[] parts = stackLine.Split(' ');
		if (parts.Length != 2)
			return false;

		path = parts[0];
		line = parts[1];
		path = path.Replace(":line", string.Empty);
		return true;
	}

	private void OnQuitClick(object sender, RoutedEventArgs e)
	{
		this.window?.Close();

		Process p = Process.GetCurrentProcess();
		p.Kill();
	}

	private void OnOkClick(object sender, RoutedEventArgs e)
	{
		this.window?.Close();
	}

	public class ErrorException : Exception
	{
		public ErrorException(Exception inner)
			: base("An error was encountered when presenting the error dialog", inner)
		{
		}
	}
}
