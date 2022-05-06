// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Extensions;
using System.Threading.Tasks;

public static class TaskExtensions
{
	public static void Run(this Task self)
	{
		Task.Run(async () =>
		{
			await self;
		});
	}
}
