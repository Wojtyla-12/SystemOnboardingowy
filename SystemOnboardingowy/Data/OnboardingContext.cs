using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Models;

namespace SystemOnboardingowy.Data
{
    // KONTEKST BAZY DANYCH (Dziedziczy po IdentityDbContext dla obsługi logowania)
    public class OnboardingContext : IdentityDbContext
    {
        public OnboardingContext(DbContextOptions<OnboardingContext> options)
            : base(options)
        {
        }

        public DbSet<Pracownik> Pracownicy { get; set; }
        public DbSet<Wdrozenie> Wdrozenia { get; set; }
        public DbSet<ZadanieWdrozeniowe> Zadania { get; set; }
    }
}