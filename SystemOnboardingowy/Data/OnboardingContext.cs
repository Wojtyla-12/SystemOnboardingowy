using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Models;

namespace SystemOnboardingowy.Data
{
    public class OnboardingContext : IdentityDbContext
    {
        public OnboardingContext(DbContextOptions<OnboardingContext> options) : base(options) { }

        public DbSet<Pracownik> Pracownicy { get; set; }
        public DbSet<Wdrozenie> Wdrozenia { get; set; }
        public DbSet<ZadanieWdrozeniowe> ZadaniaWdrozeniowe { get; set; }
        public DbSet<Odejscie> Odejscia { get; set; }
        public DbSet<Notatka> Notatki { get; set; }
    }
}