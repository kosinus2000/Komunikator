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
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wywołuje zdarzenie PropertyChanged, informując powiązany interfejs użytkownika o zmianie właściwości.
        /// </summary>
        /// <param name="propertyname">
        /// Nazwa właściwości, która uległa zmianie.
        /// Wartość domyślna jest automatycznie ustawiana przez kompilator.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
