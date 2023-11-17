using System;
using System.Windows.Input;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public abstract class MenuContextBaseCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public abstract string CommandName { get; }

        public virtual bool CanExecute(object? parameter) => true;

        public abstract void Execute(object parameter);

    }
}