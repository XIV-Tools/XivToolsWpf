// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Windows
{
	using System;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Interop;
	using System.Windows.Media;
	using System.Windows.Shapes;
	using System.Windows.Shell;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ChromedWindow : Window
	{
		private bool enableTranslucency = true;

		public ChromedWindow()
		{
			this.Background = new SolidColorBrush(Colors.Transparent);

			WindowChrome? chrome = WindowChrome.GetWindowChrome(this);

			if (chrome is null)
			{
				chrome = new WindowChrome();
				WindowChrome.SetWindowChrome(this, chrome);
			}

			chrome.NonClientFrameEdges = NonClientFrameEdges.Right | NonClientFrameEdges.Left | NonClientFrameEdges.Bottom;
			chrome.CaptionHeight = 0;

			this.Style = Application.Current.FindResource("ChromedWindowStyle") as Style;

			this.MouseDown += this.OnMouseDown;
			this.Loaded += this.OnLoaded;

			this.TitlebarForeground = new SolidColorBrush(Colors.Black);
		}

		private enum AccentState
		{
			ACCENT_DISABLED = 0,
			ACCENT_ENABLE_GRADIENT = 1,
			ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
			ACCENT_ENABLE_BLURBEHIND = 3,
			ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
			ACCENT_INVALID_STATE = 5,
		}

		private enum WindowCompositionAttribute
		{
			WCA_ACCENT_POLICY = 19,
		}

		public bool EnableTranslucency
		{
			get => this.enableTranslucency;
			set
			{
				this.enableTranslucency = value;
				this.SetTranslucency();
			}
		}

		public Brush TitlebarForeground
		{
			get;
			set;
		}

		protected override void OnActivated(EventArgs e)
		{
			this.TitlebarForeground = new SolidColorBrush(Colors.White);
			base.OnActivated(e);
		}

		protected override void OnDeactivated(EventArgs e)
		{
			if (!this.EnableTranslucency)
				this.TitlebarForeground = new SolidColorBrush(Colors.DarkGray);

			base.OnDeactivated(e);
		}

		[DllImport("user32.dll")]
		private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.SetTranslucency();
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Handled)
				return;

			if (e.ChangedButton != MouseButton.Left)
				return;

			IInputElement? element = Mouse.DirectlyOver;

			if (element is FrameworkElement obj)
			{
				if (obj.Name == "TitleBarArea" || obj.Parent == null)
				{
					this.DragMove();
				}
			}
		}

		private void SetTranslucency()
		{
			Rectangle? titlebarRect = this.GetTemplateChild("TitleBarArea") as Rectangle;
			Rectangle? backgroundRect = this.GetTemplateChild("BackgroundArea") as Rectangle;

			if (titlebarRect == null || backgroundRect == null)
				return;

			WindowInteropHelper? windowHelper = new WindowInteropHelper(this);

			AccentPolicy accent = new ();

			uint blurOpacity = 0;
			uint blurBackgroundColor = 0x000000;

			bool isWindows11 = RuntimeInformation.OSDescription == "Microsoft Windows 10.0.22000";
			bool isWindows10 = RuntimeInformation.OSDescription.StartsWith("Microsoft Windows 10");

			if (!this.EnableTranslucency || (!isWindows10 && !isWindows11))
			{
				accent.AccentState = AccentState.ACCENT_DISABLED;
				backgroundRect.Visibility = Visibility.Visible;
				backgroundRect.Opacity = 1.0;
				titlebarRect.Fill = new SolidColorBrush(Colors.Transparent);
				titlebarRect.Opacity = 1.0;
			}
			else if (isWindows11)
			{
				accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
				blurOpacity = 190;
				blurBackgroundColor = 0x303030;
				backgroundRect.Visibility = Visibility.Collapsed;
				titlebarRect.Fill = new SolidColorBrush(Colors.Transparent);
				titlebarRect.Opacity = 1.0;
			}
			else if (isWindows10)
			{
				accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
				blurOpacity = 255;
				blurBackgroundColor = 0x000000;
				backgroundRect.Visibility = Visibility.Visible;
				backgroundRect.Opacity = 0.75;
				titlebarRect.Fill = Application.Current.FindResource("MaterialDesignPaper") as SolidColorBrush;
				titlebarRect.Opacity = 0.75;
			}

			accent.GradientColor = (blurOpacity << 24) | (blurBackgroundColor & 0xFFFFFF);

			int accentStructSize = Marshal.SizeOf(accent);

			IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			WindowCompositionAttributeData data = new ();
			data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;

			SetWindowCompositionAttribute(windowHelper.Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct AccentPolicy
		{
			public AccentState AccentState;
			public uint AccentFlags;
			public uint GradientColor;
			public uint AnimationId;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WindowCompositionAttributeData
		{
			public WindowCompositionAttribute Attribute;
			public IntPtr Data;
			public int SizeOfData;
		}
	}
}
