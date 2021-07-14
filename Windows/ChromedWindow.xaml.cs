// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Windows
{
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for ChromedWindow.xaml.
	/// </summary>
	public partial class ChromedWindow : Window
	{
		public ChromedWindow(UserControl content)
		{
			this.DataContext = this;
			this.InitializeComponent();

			content.DataContext = content;
			this.Content = content;
			this.ContentArea.Content = content;
		}

		public new UserControl Content { get; set; }

		private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.DragMove();
			}
		}
	}
}
