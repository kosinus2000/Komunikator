using KomunikatorServer.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KomunikatorServer.Data

{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
    }
    

}
