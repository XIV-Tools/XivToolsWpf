// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.Commands;

using System;
using System.Windows.Input;

public class SimpleCommand : ICommand
{
	private readonly Action action;
	private readonly Func<bool> canExecute;

	public SimpleCommand(Action action, Func<bool> canExecute)
	{
		this.action = action;
		this.canExecute = canExecute;
	}

	public SimpleCommand(Action action)
	{
		this.action = action;
		this.canExecute = () => true;
	}

	public event EventHandler? CanExecuteChanged
	{
		add { CommandManager.RequerySuggested += value; }
		remove { CommandManager.RequerySuggested -= value; }
	}

	public bool CanExecute(object? parameter)
	{
		return this.canExecute.Invoke();
	}

	public void Execute(object? parameter)
	{
		this.action.Invoke();
	}
}
