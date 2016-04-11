// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: UniversalCommand.cs
// Version: 20160411

using System;
using System.Windows.Input;

namespace AutoCheckIn.Binding
{
    public class UniversalCommand : ICommand
    {
        private Func<object, bool> _canExecute;

        private Action<object> _execute;

        public UniversalCommand(Action<object> execute) : this(execute, null)
        {
        }

        public UniversalCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}