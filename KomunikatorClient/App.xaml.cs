using System.Configuration;
using System.Data;
using System.Windows;
using Serilog;         
using Serilog.Events;

namespace KomunikatorClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // Wywołanie metody bazowej

            // --- Początek konfiguracji Serilog ---
            Log.Logger = new LoggerConfiguration()
                // 1. Ustawiamy globalny minimalny poziom logowania.
                .MinimumLevel.Debug()  // Będą przetwarzane logi od Debug wzwyż.

                // 2. Kierujemy logi do okna "Debug Output" w Visual Studio.
                //    Wszystkie logi od poziomu Debug wzwyż tutaj trafią.
                .WriteTo.Debug()

                // 3. Kierujemy logi do pliku.
                .WriteTo.File(
                    path: "logs/KomunikatorClientLog-.txt", // Ścieżka do pliku (w podfolderze "logs")
                                                           // Serilog doda datę do nazwy pliku, np. KomunikatorClientLog-20250511.txt
                    rollingInterval: RollingInterval.Day,  // Nowy plik będzie tworzony każdego dnia.
                    restrictedToMinimumLevel: LogEventLevel.Information, // DO PLIKU trafią tylko logi od poziomu Information wzwyż.
                                                                         // Logi Debug nie trafią do pliku, ale będą w oknie Debug Output.
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}" // Format zapisu logu.
                )
                .CreateLogger(); // Tworzymy i przypisujemy skonfigurowany logger do globalnego Log.Logger.
            // --- Koniec konfiguracji Serilog ---

            // Testowe logi, aby sprawdzić, czy konfiguracja działa:
            Log.Information("Aplikacja KomunikatorClient została uruchomiona. Serilog jest skonfigurowany.");
        //    Log.Debug("To jest testowy log typu Debug - powinien być widoczny w Debug Output, ale nie w pliku.");
        //    Log.Warning("To jest testowy log typu Warning - powinien być wszędzie.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Ten log również powinien trafić do pliku (bo jest Information) i do Debug Output
            Log.Information("Aplikacja KomunikatorClient jest zamykana. Zapisywanie pozostałych logów.");

            Log.CloseAndFlush(); // Bardzo ważne! Zapewnia, że wszystkie buforowane logi zostaną zapisane przed zamknięciem aplikacji.

            base.OnExit(e); // Wywołanie metody bazowej
        }
    }
    
}
