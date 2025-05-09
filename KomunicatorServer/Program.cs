using Microsoft.EntityFrameworkCore;
using KomunikatorServer.Data;
using KomunikatorServer.Models;
using Microsoft.AspNetCore.Identity;

namespace KomunicatorServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var conectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(conectionString))
            {
                throw new Exception("Connection string 'DefaultConnection' not found.");
            }

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(conectionString));
            builder.Services.AddIdentity<User, IdentityRole>(options => {
                    options.Password.RequireDigit = true;               // Czy has�o musi zawiera� cyfr�? (np. 123)
                    options.Password.RequiredLength = 8;                // Minimalna d�ugo�� has�a.
                    options.Password.RequireNonAlphanumeric = false;    // Czy has�o musi zawiera� znak specjalny? (np. !, @, #). Warto ustawi� na true dla wi�kszego bezpiecze�stwa.
                    options.Password.RequireUppercase = true;           // Czy has�o musi zawiera� du�� liter�? (np. A-Z)
                    options.Password.RequireLowercase = true;           // Czy has�o musi zawiera� ma�� liter�? (np. a-z)

                    // Opcje dotycz�ce u�ytkownika
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Dozwolone znaki w nazwie u�ytkownika.
                    options.User.RequireUniqueEmail = true;        // Unikalny adres e-mail dla ka�dego u�ytkownika.


                    // Opcje dotycz�ce logowania
                    // options.SignIn.RequireConfirmedAccount = false; // Czy konto musi by� potwierdzone (np. przez email), aby si� zalogowa�. Na pocz�tek mo�na ustawi� na false.
                    // options.SignIn.RequireConfirmedEmail = false;
                    // options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            

            app.UseHttpsRedirection();

            app.UseAuthentication(); // Najpierw autentykacja
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
