using System;
using System.Threading.Tasks; 
using System.Windows.Input;

namespace KomunikatorClient.Core
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, Task> _executeAsync; 
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }


        public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_executeAsync != null)
            {
    
                _executeAsync(parameter);
            }
            else if (_execute != null)
            {
                _execute(parameter);
            }
            else
            {
                throw new InvalidOperationException("RelayCommand musi być zainicjalizowany akcją synchroniczną lub asynchroniczną.");
            }
        }
    }
}