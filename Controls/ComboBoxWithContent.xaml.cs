// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls
{
	using System.Collections;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using XivToolsWpf.DependencyProperties;

	/// <summary>
	/// Interaction logic for ComboBoxWithContent.xaml.
	/// </summary>
	public partial class ComboBoxWithContent : UserControl, INotifyPropertyChanged
	{
		public static readonly IBind<IEnumerable?> ItemsSourceDp = Binder.Register<IEnumerable?, ComboBoxWithContent>(nameof(ItemsSource), BindMode.OneWay);
		public static readonly IBind<object?> SelectedItemDp = Binder.Register<object?, ComboBoxWithContent>(nameof(SelectedItem), BindMode.TwoWay);
		public static readonly IBind<DataTemplate> ComboBoxItemTemplateDp = Binder.Register<DataTemplate, ComboBoxWithContent>(nameof(ComboBoxItemTemplate));
		public static readonly IBind<DataTemplate?> ContentTemplateDp = Binder.Register<DataTemplate?, ComboBoxWithContent>(nameof(ContentTemplate));
		public static readonly IBind<string?> HintDp = Binder.Register<string?, ComboBoxWithContent>(nameof(Hint));

		public ComboBoxWithContent()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public IEnumerable? ItemsSource
		{
			get => ItemsSourceDp.Get(this);
			set => ItemsSourceDp.Set(this, value);
		}

		public object? SelectedItem
		{
			get => SelectedItemDp.Get(this);
			set => SelectedItemDp.Set(this, value);
		}

		public DataTemplate ComboBoxItemTemplate
		{
			get => ComboBoxItemTemplateDp.Get(this);
			set => ComboBoxItemTemplateDp.Set(this, value);
		}

		public new DataTemplate? ContentTemplate
		{
			get => ContentTemplateDp.Get(this);
			set => ContentTemplateDp.Set(this, value);
		}

		public string? Hint
		{
			get => HintDp.Get(this);
			set => HintDp.Set(this, value);
		}

		public FrameworkElement? SelectedContent { get; set; }

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.ContentTemplate == null)
			{
				this.SelectedContent = new ContentPresenter();
			}
			else
			{
				this.SelectedContent = (FrameworkElement)this.ContentTemplate.LoadContent();
			}

			this.SelectedContent.DataContext = this.SelectedItem;
		}
	}
}
