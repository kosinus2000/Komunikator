using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KomunikatorClient.Core
{
    /// <summary>
    /// Implementacja interfejsu ICommand umożliwiająca przekazywanie logiki wykonania oraz warunku wykonania komendy.
    /// Używana głównie w architekturze MVVM do powiązania poleceń z widokiem.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        /// <summary>
        /// Zdarzenie, które jest wywoływane, gdy system powinien sprawdzić, czy komenda może zostać wykonana.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Tworzy nową instancję RelayCommand.
        /// </summary>
        /// <param name="execute">Akcja, która ma być wykonana.</param>
        /// <param name="canExecute">Funkcja określająca, czy komenda może być wykonana. Opcjonalna.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Określa, czy komenda może zostać wykonana.
        /// </summary>
        /// <param name="parameter">Parametr przekazywany do komendy.</param>
        /// <returns>True, jeśli komenda może być wykonana; w przeciwnym razie false.</returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        /// <summary>
        /// Wykonuje akcję przypisaną do komendy.
        /// </summary>
        /// <param name="parameter">Parametr przekazywany do komendy.</param>
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

    }
}
