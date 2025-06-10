using System.Security.Cryptography.X509Certificates;
using KomunikatorServer.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KomunikatorServer.Data
{
    /// <summary>
    /// Klasa kontekstu bazy danych dla aplikacji komunikatora.
    /// Dziedziczy po <see cref="IdentityDbContext{TUser}"/> i zarządza encjami związanymi z użytkownikami i kontaktami.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        /// <summary>
        /// Reprezentuje relacje kontaktów między użytkownikami.
        /// </summary>
        public DbSet<UserContact> UserContacts { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">Opcje konfiguracji kontekstu bazy danych.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Konfiguruje źródło danych oraz inne ustawienia kontekstu.
        /// Można rozszerzyć tę metodę o dodatkowe ustawienia, np. konfigurację loggera lub dostawcy bazy danych.
        /// </summary>
        /// <param name="optionsBuilder">Obiekt używany do konfiguracji opcji kontekstu.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        /// <summary>
        /// Konfiguruje model danych za pomocą Fluent API.
        /// Określa m.in. klucze główne, relacje i strategie usuwania danych.
        /// </summary>
        /// <param name="builder">Obiekt używany do tworzenia modelu danych.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserContact>()
                .HasKey(uc => new { uc.UserId, uc.ContactId });
            
            // Konfiguracja klucza głównego w tabeli UserContacts (złożony klucz).
            builder.Entity<UserContact>()
                .HasOne(uc => uc.User)       // UserContact ma jednego Usera
                .WithMany(u => u.AddedContacts) // User ma wiele AddedContacts
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserContact>()
                .HasOne(uc => uc.Contact)    // UserContact ma jednego Contacta
                .WithMany(u => u.IsContactOf) // Contact jest w wielu IsContactOf
                .HasForeignKey(uc => uc.ContactId)
                .OnDelete(DeleteBehavior.Restrict);


            // Konfiguracja ChatMessage
            builder.Entity<ChatMessage>()
                .HasKey(cm => cm.Id);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)      // Wiadomość ma jednego Nadawcę
                .WithMany(u => u.SentMessages) // Nadawca ma wiele wysłanych wiadomości
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.Receiver)    // Wiadomość ma jednego Odbiorcę
                .WithMany(u => u.ReceivedMessages) // Odbiorca ma wiele odebranych wiadomości
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);


            
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
        }
    }
}