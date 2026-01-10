using Microsoft.AspNetCore.Identity;
using SystemOnboardingowy.Models;

namespace SystemOnboardingowy.Data
{
    // KLASA INICJALIZUJĄCA ROLE I UŻYTKOWNIKÓW
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Tworzenie ról
            string[] roleNames = { "Kierownik", "HR", "IT", "Sprzet" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tworzenie użytkowników testowych (Hasło: Haslo123!)
            await CreateUser(userManager, "kierownik@firma.pl", "Kierownik");
            await CreateUser(userManager, "hr@firma.pl", "HR");
            await CreateUser(userManager, "it@firma.pl", "IT");
            await CreateUser(userManager, "sprzet@firma.pl", "Sprzet");
        }

        private static async Task CreateUser(UserManager<IdentityUser> userManager, string email, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, "Haslo123!");
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}