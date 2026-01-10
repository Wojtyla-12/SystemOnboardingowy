using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using SystemOnboardingowy.Models;

namespace SystemOnboardingowy.Data
{
    // Zakładam, że dziedziczysz po IdentityDbContext jeśli masz logowanie, 
    // lub po prostu DbContext jeśli na razie robimy bez ról w kodzie.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pracownik> Pracownicy { get; set; }
        public DbSet<Wdrozenie> Wdrozenia { get; set; }
        public DbSet<ZadanieWdrozeniowe> Zadania { get; set; }
    }
}