using KomunikatorServer.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KomunikatorServer.Data
{
    /// <summary>
    /// Klasa kontekstu bazy danych dla aplikacji, dziedzicząca po <see cref="IdentityDbContext{TUser}"/>.
    /// Jest odpowiedzialna za zarządzanie tożsamością użytkowników (<see cref="AppUser"/>) oraz danymi aplikacji
    /// za pomocą Entity Framework Core.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        /// <summary>
        /// Konstruktor klasy <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">Opcje konfiguracji kontekstu bazy danych.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        /// <summary>
        /// Konfiguruje opcje kontekstu bazy danych.
        /// Ta metoda może być nadpisana w celu dostosowania konfiguracji bazy danych,
        /// np. wyboru dostawcy bazy danych lub ustawień połączenia.
        /// </summary>
        /// <param name="optionsBuilder">Obiekt <see cref="DbContextOptionsBuilder"/> używany do konfiguracji kontekstu.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Można tu dodać dodatkowe konfiguracje, jeśli są wymagane.
            // Przykład: optionsBuilder.UseSqlServer("ConnectionStrings:DefaultConnection");
        }
    }
}