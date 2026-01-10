using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Data;

var builder = WebApplication.CreateBuilder(args);

// KONFIGURACJA: Połączenie z bazą SQL
builder.Services.AddDbContext<OnboardingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OnboardingContext")));

// KONFIGURACJA: Identity (Użytkownicy i Role) - uproszczone hasła
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<OnboardingContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// AUTOMATYZACJA: Seedowanie bazy danych przy starcie
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.Initialize(services, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Błąd seedowania: " + ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // KTO?
app.UseAuthorization();  // CO MOŻE?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Wdrozenia}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();


//Kierownik: kierownik @firma.pl / Haslo123!(Może dodawać pracowników i wdrożenia)
//IT: it @firma.pl / Haslo123!(Może odhaczać tylko zadania IT)
//HR: hr @firma.pl / Haslo123!(Może odhaczać tylko zadania HR)
//Login: sprzet@firma.pl  Hasło: Haslo123!