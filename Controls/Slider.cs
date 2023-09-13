// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Controls;

using System.Reflection;
using System.Windows.Input;

public class Slider : System.Windows.Controls.Slider
{
	private MethodInfo? moveToNextTickMethod;

	protected double GetChangeMultiplier()
	{
		if (Keyboard.IsKeyDown(Key.LeftShift))
			return 10;

		if (Keyboard.IsKeyDown(Key.RightShift))
			return 10;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			return 0.1f;

		if (Keyboard.IsKeyDown(Key.RightCtrl))
			return 0.1f;

		return 1.0;
	}

	protected override void OnDecreaseSmall()
	{
		this.MoveToNextTick(-this.SmallChange * this.GetChangeMultiplier());
	}

	protected override void OnIncreaseSmall()
	{
		this.MoveToNextTick(this.SmallChange * this.GetChangeMultiplier());
	}

	protected override void OnDecreaseLarge()
	{
		this.MoveToNextTick(-this.LargeChange * this.GetChangeMultiplier());
	}

	protected override void OnIncreaseLarge()
	{
		this.MoveToNextTick(this.LargeChange * this.GetChangeMultiplier());
	}

	protected void MoveToNextTick(double direction)
	{
		if (this.moveToNextTickMethod == null)
			this.moveToNextTickMethod = typeof(System.Windows.Controls.Slider).GetMethod("MoveToNextTick", BindingFlags.NonPublic | BindingFlags.Instance);

		if (this.moveToNextTickMethod == null)
			return;

		this.moveToNextTickMethod.Invoke(this, new object[] { direction });
	}
}
