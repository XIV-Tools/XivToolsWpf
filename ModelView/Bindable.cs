// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.ModelView
{
	using System;
	using System.ComponentModel;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public abstract class Bindable : INotifyPropertyChanged
	{
		private readonly PropertyStore propertyStore;

		protected Bindable()
		{
			this.propertyStore = new PropertyStore();
			this.propertyStore.PropertyChanged += this.RaisePropertyChanged;

			PropertyInfo[] properties = this.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				object? defaultValue = null;

				if (property.PropertyType.IsValueType)
					defaultValue = Activator.CreateInstance(property.PropertyType);

				this.propertyStore.RegisterProperty(property.Name, property.PropertyType, defaultValue);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected bool IsPropertyChangedEventInvokingEnabled
		{
			get => this.propertyStore.IsPropertyChangedEventInvokingEnabled;
			set => this.propertyStore.IsPropertyChangedEventInvokingEnabled = value;
		}

		protected bool IsPropertyChangedCallbackInvokingEnabled
		{
			get => this.propertyStore.IsPropertyChangedCallbackInvokingEnabled;
			set => this.propertyStore.IsPropertyChangedCallbackInvokingEnabled = value;
		}

		protected object? GetValue([CallerMemberName] string propertyName = null)
		{
			return this.propertyStore.GetValue(propertyName);
		}

		protected void ForceSetValue(object value, [CallerMemberName] string propertyName = null)
		{
			this.propertyStore.ForceSetValue(value, propertyName);
		}

		protected void SetValue(object value, [CallerMemberName] string propertyName = null)
		{
			this.propertyStore.SetValue(value, propertyName);
		}

		protected void RaisePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			this.PropertyChanged?.Invoke(this, e);
		}

		protected void RaisePropertyChanged(PropertyChangedEventArgs e)
		{
			this.PropertyChanged?.Invoke(this, e);
		}
	}
}
