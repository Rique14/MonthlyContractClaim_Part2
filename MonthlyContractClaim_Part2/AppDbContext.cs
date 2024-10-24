using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MonthlyClaimContractSystem_Part2
{
    public class AppDbContext : DbContext
    {
        public DbSet<Claim> Claims { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use MySQL connection with Pomelo
            optionsBuilder.UseMySql("Server=localhost;Database=ContractClaimsDB;User=root;Password=;",
                new MySqlServerVersion(new Version(8, 0, 21))); // Specify MySQL version
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claim>().ToTable("Claims");
        }
    }
}