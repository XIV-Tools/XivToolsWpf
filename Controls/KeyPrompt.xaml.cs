// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using FontAwesome.Sharp;
using FontAwesome.Sharp.Pro;
using PropertyChanged.SourceGenerator;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf.DependencyProperties;

public partial class KeyPrompt : UserControl
{
	public static readonly IBind<Key> KeyDp = Binder.Register<Key, KeyPrompt>(nameof(Key), OnKeyChanged);

	[Notify] private string? label;
	[Notify] private ProIcons? icon;
	[Notify] private int iconRotation;
	[Notify] private IconStyles iconStyle;

	public KeyPrompt()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.Key = Key.LeftShift;
	}

	public Key Key
	{
		get => KeyDp.Get(this);
		set => KeyDp.Set(this, value);
	}

	private static void OnKeyChanged(KeyPrompt sender, Key key)
	{
		sender.Label = null;
		sender.Icon = null;
		sender.IconRotation = 0;
		sender.IconStyle = IconStyles.Solid;

		if (key == Key.Return)
		{
			sender.Icon = ProIcons.LevelDown;
			sender.IconRotation = 90;
		}
		else if (key == Key.Tab)
		{
			sender.Icon = ProIcons.Exchange;
		}
		else if (key == Key.LeftShift)
		{
			sender.Icon = ProIcons.ArrowAltUp;
			sender.IconStyle = IconStyles.OutlineThin;
			sender.Label = "L";
		}
		else if (key == Key.RightShift)
		{
			sender.Icon = ProIcons.ArrowAltUp;
			sender.IconStyle = IconStyles.OutlineThin;
			sender.Label = "R";
		}
		else
		{
			sender.Label = key.ToString().ToUpper();
		}
	}
}