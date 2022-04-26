// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyInjection;

using System;

public interface ILocaleProvider : IDependency
{
	event LocalizationEvent? LocaleChanged;

	bool Loaded { get; }

	bool HasString(string key);
	string GetStringFormatted(string key, params string[] param);
	string GetStringAllLanguages(string key);
	string GetString(string key, bool silent = false);
}

#pragma warning disable SA1201
public delegate void LocalizationEvent();
