// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System.ComponentModel;
using System.Windows.Controls;

public partial class InfoControl : UserControl, INotifyPropertyChanged
{
	private bool isError;

	public InfoControl()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		this.IsError = false;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Text
	{
		get => this.TextBlock.Text;
		set => this.TextBlock.Text = value;
	}

	public bool IsError
	{
		get => this.isError;
		set
		{
			this.isError = value;
			this.PropertyChanged?.Invoke(this, new(nameof(InfoControl.IsError)));
		}
	}
}
