// © XIV-Tools.
// Licensed under the MIT license.

namespace System.Windows;

using System;
using System.Windows.Media.Animation;

public static class FrameworkElementExtensions
{
	public static void Animate(this FrameworkElement self, DependencyProperty property, double to, int durationMs)
	{
		Storyboard story = new Storyboard();
		DoubleAnimation anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(durationMs)));
		Storyboard.SetTarget(anim, self);
		Storyboard.SetTargetProperty(anim, new PropertyPath(property));
		story.Children.Add(anim);
		story.Begin();
	}

	public static T GetResource<T>(this FrameworkElement self, string name)
	{
		if (!self.Resources.Contains(name))
			throw new Exception($"Resource with Key: \"{name}\" not found on element: {self}");

		object resource = self.Resources[name];

		if (resource is not T tResource)
			throw new Exception($"Resource with Key: \"{name}\" is not of type: {typeof(T)}");

		return tResource;
	}

	public static void BeginStoryboard(this FrameworkElement self, string name, double speed = 1.0)
	{
		Storyboard sb = self.GetResource<Storyboard>(name);
		sb.Begin();
		sb.SpeedRatio = speed;
	}

	public static void StopStoryboard(this FrameworkElement self, string name)
	{
		Storyboard sb = self.GetResource<Storyboard>(name);
		sb.Stop();
	}
}
