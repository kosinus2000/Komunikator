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
                    options.Password.RequireDigit = true;               // Czy has³o musi zawieraæ cyfrê? (np. 123)
                    options.Password.RequiredLength = 8;                // Minimalna d³ugoœæ has³a.
                    options.Password.RequireNonAlphanumeric = false;    // Czy has³o musi zawieraæ znak specjalny? (np. !, @, #). Warto ustawiæ na true dla wiêkszego bezpieczeñstwa.
                    options.Password.RequireUppercase = true;           // Czy has³o musi zawieraæ du¿¹ literê? (np. A-Z)
                    options.Password.RequireLowercase = true;           // Czy has³o musi zawieraæ ma³¹ literê? (np. a-z)

                    // Opcje dotycz¹ce u¿ytkownika
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Dozwolone znaki w nazwie u¿ytkownika.
                    options.User.RequireUniqueEmail = true;        // Unikalny adres e-mail dla ka¿dego u¿ytkownika.


                    // Opcje dotycz¹ce logowania
                    // options.SignIn.RequireConfirmedAccount = false; // Czy konto musi byæ potwierdzone (np. przez email), aby siê zalogowaæ. Na pocz¹tek mo¿na ustawiæ na false.
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
