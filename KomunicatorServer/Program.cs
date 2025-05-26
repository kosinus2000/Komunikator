using Microsoft.EntityFrameworkCore;
using KomunikatorServer.Data;
using Microsoft.AspNetCore.Identity;
using IdentityUser = KomunikatorServer.DTOs.IdentityUser;

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
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
                    options.Password.RequireDigit = true;               
                    options.Password.RequiredLength = 8;                
                    options.Password.RequireNonAlphanumeric = false;   
                    options.Password.RequireUppercase = true;           
                    options.Password.RequireLowercase = true;          

                    // Opcje dotyczace uzytkownika
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; 
                    options.User.RequireUniqueEmail = true;        


                    // Opcje dotyczace logowania
                    // options.SignIn.RequireConfirmedAccount = false; // Czy konto musi byc potwierdzone (np. przez email), aby sie zalogowac. Na poczatek mozna ustawic na false.
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
