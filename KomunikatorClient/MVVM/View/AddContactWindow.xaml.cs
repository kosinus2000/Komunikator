using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KomunikatorClient.Services;
using KomunikatorServer.DTOs;
using KomunikatorShared.DTOs;

namespace KomunikatorClient.MVVM.View
{
    /// <summary>
    /// Logika interakcji dla AddContactWindow.xaml
    /// </summary>
    public partial class AddContactWindow : Window
    {
        /// <summary>
        /// Kolekcja wyników wyszukiwania użytkowników.
        /// </summary>
        public ObservableCollection<UserSearchDto> SearchResults { get; set; } = new ObservableCollection<UserSearchDto>();

        private readonly HttpClient _httpClient;
        private readonly CurrentUserSessionService _sessionService;

        /// <summary>
        /// Zdarzenie wywoływane po pomyślnym dodaniu nowego kontaktu.
        /// </summary>
        public event EventHandler<ContactAddedEventArgs>? ContactAdded;

        /// <summary>
        /// Inicjalizuje nową instancję okna AddContactWindow.
        /// </summary>
        /// <param name="httpClient">Instancja HttpClient skonfigurowana do komunikacji z API.</param>
        /// <param name="sessionService">Serwis zarządzający bieżącą sesją użytkownika.</param>
        public AddContactWindow(HttpClient httpClient, CurrentUserSessionService sessionService)
        {
            InitializeComponent();
            btnMaximize.IsEnabled = false;
            this.DataContext = this;
            SearchResultsListBox.ItemsSource = SearchResults;

            _httpClient = httpClient;
            _sessionService = sessionService;

            var jwtToken = _sessionService.CurrentUser?.Token;

            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            else
            {
                MessageBox.Show("Błąd: Token autoryzacji jest niedostępny. Zaloguj się ponownie.", "Błąd autoryzacji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku "Exit" (X).
        /// </summary>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku "Minimize" (-).
        /// </summary>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Obsługuje zdarzenie naciśnięcia lewego przycisku myszy na elemencie UI,
        /// umożliwiając przeciąganie okna.
        /// </summary>
        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Obsługuje zdarzenie zmiany tekstu w polu wyszukiwania.
        /// </summary>
        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku "Szukaj".
        /// </summary>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTermTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                StatusMessageTextBlock.Text = "Wprowadź frazę do wyszukania.";
                SearchResults.Clear();
                return;
            }

            StatusMessageTextBlock.Text = "Wyszukiwanie...";
            SearchResults.Clear();

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("Contacts/searchUsers?searchTerm=" + Uri.EscapeDataString(searchTerm));

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<UserSearchDto>>();
                    if (users != null && users.Any())
                    {
                        foreach (var user in users)
                        {
                            SearchResults.Add(user);
                        }
                        StatusMessageTextBlock.Text = $"Znaleziono {users.Count} użytkowników.";
                    }
                    else
                    {
                        StatusMessageTextBlock.Text = "Nie znaleziono użytkowników pasujących do kryteriów.";
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Błąd wyszukiwania użytkowników. Status: {response.StatusCode}, Odpowiedź: {errorContent}");
                    StatusMessageTextBlock.Text = $"Błąd wyszukiwania: {response.ReasonPhrase}.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Błąd HTTP podczas wyszukiwania użytkowników: {httpEx.Message}");
                StatusMessageTextBlock.Text = $"Błąd połączenia z serwerem: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wystąpił nieoczekiwany błąd podczas wyszukiwania użytkowników: {ex.Message}");
                StatusMessageTextBlock.Text = $"Wystąpił błąd: {ex.Message}";
            }
        }

        /// <summary>
        /// Obsługuje kliknięcie przycisku "Dodaj" obok wyszukanego użytkownika.
        /// </summary>
        private async void AddContactFromSearch_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button == null) return;

            UserSearchDto? selectedUser = button.Tag as UserSearchDto;
            if (selectedUser == null)
            {
                StatusMessageTextBlock.Text = "Błąd: nie można pobrać danych użytkownika do dodania.";
                return;
            }

            StatusMessageTextBlock.Text = $"Dodawanie {selectedUser.Username} do kontaktów...";

            try
            {
                var requestDto = new AddContactRequestDto { TargetUsername = selectedUser.Username };
                var jsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestDto),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await _httpClient.PostAsync("Contacts/addContact", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessageTextBlock.Text = $"Pomyślnie dodano {selectedUser.Username} do kontaktów!";
                    SearchResults.Remove(selectedUser);

                    ContactDto? addedContact = null;
                    try
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(responseBody))
                        {
                            if (doc.RootElement.TryGetProperty("contact", out JsonElement contactElement))
                            {
                                addedContact = System.Text.Json.JsonSerializer.Deserialize<ContactDto>(contactElement.GetRawText());
                            }
                        }
                    }
                    catch (System.Text.Json.JsonException jsonEx)
                    {
                        Debug.WriteLine($"Błąd parsowania odpowiedzi serwera po dodaniu kontaktu: {jsonEx.Message}");
                    }

                    ContactAdded?.Invoke(this, new ContactAddedEventArgs(addedContact));
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    string errorMessage = "Wystąpił błąd.";
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(errorContent))
                        {
                            if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement) && messageElement.ValueKind == JsonValueKind.String)
                            {
                                errorMessage = messageElement.GetString() ?? errorMessage;
                            }
                            else if (doc.RootElement.TryGetProperty("Message", out JsonElement msgElement) && msgElement.ValueKind == JsonValueKind.String)
                            {
                                errorMessage = msgElement.GetString() ?? errorMessage;
                            }
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        errorMessage = $"Błąd: {response.ReasonPhrase}. Odpowiedź: {errorContent}";
                    }

                    Debug.WriteLine($"Błąd dodawania kontaktu. Status: {response.StatusCode}, Odpowiedź: {errorContent}");
                    StatusMessageTextBlock.Text = $"Błąd dodawania: {errorMessage}";
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Błąd HTTP podczas dodawania kontaktu: {httpEx.Message}");
                StatusMessageTextBlock.Text = $"Błąd połączenia z serwerem: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wystąpił nieoczekiwany błąd podczas dodawania kontaktu: {ex.Message}");
                StatusMessageTextBlock.Text = $"Wystąpił błąd: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Klasa argumentów zdarzenia dla zdarzenia ContactAdded.
    /// Zawiera informacje o dodanym kontakcie.
    /// </summary>
    public class ContactAddedEventArgs : EventArgs
    {
        public ContactDto? AddedContact { get; }

        public ContactAddedEventArgs(ContactDto? addedContact)
        {
            AddedContact = addedContact;
        }
    }
}