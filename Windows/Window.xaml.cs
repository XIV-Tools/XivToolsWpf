// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Windows
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for Window.xaml.
	/// </summary>
	public partial class StyledWindow : Window
	{
		private bool overlapTitleBar;

		private StyledWindow(UserControl content)
		{
			this.DataContext = this;
			this.InitializeComponent();

			content.DataContext = content;
			this.Content = content;
			this.ContentArea.Content = content;
		}

		public new UserControl Content { get; set; }

		public bool UseCustomBorder { get; set; } = true;

		public bool OverlapTitleBar
		{
			get
			{
				return this.overlapTitleBar;
			}
			set
			{
				this.overlapTitleBar = value;
				Thickness margin = this.ContentArea.Margin;
				margin.Top = -this.TitleBar.Height;
				this.ContentArea.Margin = margin;
			}
		}

		public static StyledWindow Create<T>()
			where T : UserControl
		{
			UserControl content = Activator.CreateInstance<T>();
			return new StyledWindow(content);
		}

		public static StyledWindow Show<T>()
			where T : UserControl
		{
			StyledWindow window = Create<T>();
			window.Show();
			return window;
		}

		private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.DragMove();
			}
		}

		private void OnResizeDrag(object sender, DragDeltaEventArgs e)
		{
		}

		private void OnWindowActivated(object sender, EventArgs e)
		{
			if (!this.UseCustomBorder)
			{
				this.ActiveBorder.Visibility = Visibility.Collapsed;
				return;
			}

			this.ActiveBorder.Visibility = Visibility.Visible;
		}

		private void OnWindowDeactivated(object sender, EventArgs e)
		{
			this.ActiveBorder.Visibility = Visibility.Collapsed;
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OnMinimiseClick(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}
	}
}
