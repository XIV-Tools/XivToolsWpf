// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Utils;

using System;
using System.Threading.Tasks;

public class FuncQueue
{
	private readonly Func<Task> func;

	private int currentDelayValue;
	private Task? task;

	public FuncQueue(Func<Task> func, int delay)
	{
		this.Delay = delay;
		this.func = func;
	}

	public int Delay { get; set; }
	public bool Pending { get; private set; }
	public bool Executing { get; private set; }

	public async Task WaitForPendingExecute()
	{
		this.ClearDelay();

		while (this.Pending || this.Executing)
		{
			await Task.Delay(1);
		}
	}

	public void Invoke()
	{
		this.currentDelayValue = this.Delay;

		if (this.task == null || this.task.IsCompleted)
		{
			this.task = Task.Run(this.RunTask);
		}
	}

	public void ClearDelay()
	{
		this.currentDelayValue = 0;
	}

	public void InvokeImmediate()
	{
		this.Invoke();
		this.ClearDelay();
	}

	private async Task RunTask()
	{
		// Double loops to handle case where a refresh delay was added
		// while the refresh was running
		while (this.currentDelayValue >= 0)
		{
			lock (this)
				this.Pending = true;

			while (this.currentDelayValue > 0)
			{
				await Task.Delay(10);
				this.currentDelayValue -= 10;
			}

			lock (this)
			{
				this.Executing = true;
				this.Pending = false;
			}

			await this.func.Invoke();
			this.currentDelayValue -= 1;

			lock (this)
			{
				this.Executing = false;
			}
		}
	}
}
