// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System.Windows.Controls;
using FontAwesome.Sharp;
using FontAwesome.Sharp.Pro;
using XivToolsWpf.DependencyProperties;

public partial class Header : UserControl
{
	public static readonly IBind<ProIcons> IconDp = Binder.Register<ProIcons, Header>(nameof(Icon));
	public static readonly IBind<string> TextDp = Binder.Register<string, Header>(nameof(Text));

	public Header()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ProIcons Icon
	{
		get => IconDp.Get(this);
		set => IconDp.Set(this, value);
	}

	public string Text
	{
		get => TextDp.Get(this);
		set => TextDp.Set(this, value);
	}
}
