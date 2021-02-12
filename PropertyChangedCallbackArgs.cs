// © XIV-Tools.
// Licensed under the MIT license.

// MIT License
//
// Copyright (c) 2017 Marian Dolinský
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
namespace Wpf.Mv
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Callback data containing info about the changed property in the <see cref="PropertyChangedCallbackHandler"/>.
	/// </summary>
	public sealed class PropertyChangedCallbackArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyChangedCallbackArgs"/> class.
		/// </summary>
		/// <param name="oldValue">Previous value of the changed property.</param>
		/// <param name="newValue">Current value of the changed property.</param>
		public PropertyChangedCallbackArgs(object oldValue, object newValue)
		{
			this.Handled = false;
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}

		/// <summary>
		/// Gets or sets a value indicating whether callback has been handled.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// Gets the previous value of the changed property.
		/// </summary>
		public object OldValue { get; }

		/// <summary>
		/// Gets the current value of the changed property.
		/// </summary>
		public object NewValue { get; }
	}
}
