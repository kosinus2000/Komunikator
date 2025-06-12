using System.Windows;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using KomunikatorClient.Services;
using KomunikatorClient.MVVM.View;
using KomunikatorClient.MVVM.ViewModel;
using System.Net.Http;

namespace KomunikatorClient
{
    /// <summary>
    /// Logika interakcji dla App.xaml.
    /// Klasa główna aplikacji, odpowiedzialna za konfigurację usług, logowania oraz zarządzanie cyklem życia aplikacji.
    /// Dziedziczy po <see cref="Application"/>.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Statyczna właściwość udostępniająca dostawcę usług (Service Provider)
        /// umożliwiającego rozwiązywanie zależności (Dependency Injection) w całej aplikacji.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Metoda wywoływana przy starcie aplikacji.
        /// Odpowiada za konfigurację wstrzykiwania zależności (DI) oraz systemu logowania Serilog.
        /// </summary>
        /// <param name="e">Argumenty zdarzenia startowego.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddTransient<LoginWindow>();
            serviceCollection.AddSingleton<AuthService>();
            serviceCollection.AddSingleton<CurrentUserSessionService>();
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<KomunikatorClient.MVVM.ViewModel.MainViewModel>();
            serviceCollection.AddSingleton<UserService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);

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

            Log.Information("Aplikacja KomunikatorClient została uruchomiona. Serwisy zarejestrowane, Serilog skonfigurowany.");
        }

        /// <summary>
        /// Metoda wywoływana przy zamykaniu aplikacji.
        /// Odpowiada za prawidłowe zamknięcie i opróżnienie buforów loggera Serilog.
        /// </summary>
        /// <param name="e">Argumenty zdarzenia zamknięcia.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Aplikacja KomunikatorClient jest zamykana. Zapisywanie pozostałych logów.");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}