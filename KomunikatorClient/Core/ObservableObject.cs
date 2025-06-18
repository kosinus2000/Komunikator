using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KomunikatorClient.Core
{
    /// <summary>
    /// Klasa bazowa implementująca INotifyPropertyChanged.
    /// Umożliwia automatyczne powiadamianie o zmianie właściwości w modelach danych.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Zdarzenie wywoływane, gdy wartość właściwości ulegnie zmianie.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Wywołuje zdarzenie PropertyChanged, informując powiązany interfejs użytkownika o zmianie właściwości.
        /// </summary>
        /// <param name="propertyName">
        /// Nazwa właściwości, która uległa zmianie.
        /// Wartość domyślna jest automatycznie ustawiana przez kompilator.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Ustawia wartość właściwości i wywołuje zdarzenie PropertyChanged, jeśli wartość uległa zmianie.
        /// </summary>
        /// <typeparam name="T">Typ właściwości.</typeparam>
        /// <param name="field">Referencja do pola backing field właściwości.</param>
        /// <param name="value">Nowa wartość właściwości.</param>
        /// <param name="propertyName">Nazwa właściwości. Domyślnie ustawiana przez kompilator.</param>
        /// <returns>True, jeśli wartość właściwości została zmieniona; w przeciwnym razie false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}