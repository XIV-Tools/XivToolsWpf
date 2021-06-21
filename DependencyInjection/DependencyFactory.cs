// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyInjection
{
	using System;
	using System.Collections.Generic;

	public static class DependencyFactory
	{
		private static readonly Dictionary<Type, IDependency> Dependencies = new Dictionary<Type, IDependency>();

		public static void RegisterDependency<T>(T dependancy)
			where T : IDependency
		{
			if (Dependencies.ContainsKey(typeof(T)))
				throw new Exception("Attempt to register duplicate dependency: " + typeof(T));

			Dependencies.Add(typeof(T), dependancy);
		}

		public static T GetDependency<T>()
			where T : IDependency
		{
			IDependency? dep;
			if (!Dependencies.TryGetValue(typeof(T), out dep))
				throw new Exception($"No dependency registered for type: {typeof(T)}");

			return (T)dep;
		}
	}
}
