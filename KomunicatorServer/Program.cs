using System.Text;
using Microsoft.EntityFrameworkCore;
using KomunikatorServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // Wa¿noœæ wystawcy tokena
                        ValidateAudience = true, // Wa¿noœæ odbiorcy tokena
                        ValidateLifetime = true, // Wa¿noœæ czasu ¿ycia tokena (exp, nbf)
                        ValidateIssuerSigningKey = true, // Wa¿noœæ klucza szyfruj¹cego

                        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Wczytuje z appsettings.json
                        ValidAudience = builder.Configuration["Jwt:Audience"], // Wczytuje z appsettings.json
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Wczytuje klucz z appsettings.json
                    };
                });

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
