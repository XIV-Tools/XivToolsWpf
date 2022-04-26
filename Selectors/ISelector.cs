// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Selectors;

public delegate void SelectorSelectedEvent(bool close);

public interface ISelector
{
	event SelectorSelectedEvent? SelectionChanged;
}

public interface ISelectorView : ISelector
{
	Selector Selector { get; }
}
