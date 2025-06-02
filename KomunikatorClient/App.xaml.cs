using System.Windows;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection; 
using System; 
using KomunikatorClient.Services;
using KomunikatorClient.DTOs;
using KomunikatorClient.MVVM.View;
using KomunikatorClient.MVVM.ViewModel;

namespace KomunikatorClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 2. Utworzenie ServiceCollection
            var serviceCollection = new ServiceCollection();

            // 3. Rejestracja serwisów jako Singleton
            // Singleton oznacza, że będzie tylko jedna instancja tej klasy w całej aplikacji.
            serviceCollection.AddSingleton<AuthService>();
            serviceCollection.AddSingleton<CurrentUserSessionService>();
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<MainViewModel>();
            // Możesz tu dodać inne serwisy, które chcesz rozwiązywać przez DI


            ServiceProvider = serviceCollection.BuildServiceProvider();


            base.OnStartup(e);

            // --- Początek konfiguracji Serilog ---
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File(
                    path: "logs/KomunikatorClientLog-.txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
            // --- Koniec konfiguracji Serilog ---

            Log.Information("Aplikacja KomunikatorClient została uruchomiona. Serwisy zarejestrowane, Serilog skonfigurowany.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Aplikacja KomunikatorClient jest zamykana. Zapisywanie pozostałych logów.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}