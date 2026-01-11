using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Data;
using SystemOnboardingowy.Models;

namespace SystemOnboardingowy.Controllers
{
    [Authorize]
    public class OdejsciaController : Controller
    {
        private readonly OnboardingContext _context;

        public OdejsciaController(OnboardingContext context)
        {
            _context = context;
        }

        // GET: Odejscia
        public async Task<IActionResult> Index(bool pokazZakonczone = false)
        {
            var query = _context.Odejscia
                .Include(o => o.Pracownik)
                .Include(o => o.Zadania)
                .AsQueryable();

            if (!pokazZakonczone)
            {
                query = query.Where(o => o.Status != StatusZgloszenia.Zakonczone && o.Status != StatusZgloszenia.Anulowane);
            }

            var lista = await query
                .OrderByDescending(o => o.Status == StatusZgloszenia.Nowe)
                .ThenBy(o => o.DataOdejscia)
                .ToListAsync();

            ViewData["PokazZakonczone"] = pokazZakonczone;

            return View(lista);
        }

        // GET: Odejscia/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var odejscie = await _context.Odejscia
                .Include(o => o.Pracownik)
                .Include(o => o.Notatki)
                .Include(o => o.Zadania)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (odejscie == null)
            {
                return NotFound();
            }

            odejscie.Notatki = odejscie.Notatki.OrderByDescending(n => n.DataUtworzenia).ToList();

            return View(odejscie);
        }

        // GET: Odejscia/Create
        [Authorize(Roles = "Kierownik")]
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownicy.Where(p => !p.CzyZarchiwizowany), "Id", "ImieNazwisko");
            return View();
        }

        // POST: Odejscia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> Create([Bind("Id,DataOdejscia,CzyPrzekierowacPoczte,AdresatPrzekierowania,InstrukcjaDlaPlikow,PracownikId")] Odejscie odejscie)
        {
            if (ModelState.IsValid)
            {
                odejscie.DataUtworzenia = DateTime.Now;
                odejscie.Status = StatusZgloszenia.Nowe;

                _context.Add(odejscie);
                await _context.SaveChangesAsync();

                var ostatnieWdrozenie = await _context.Wdrozenia
                    .Include(w => w.Zadania)
                    .OrderByDescending(w => w.DataUtworzenia)
                    .FirstOrDefaultAsync(w => w.PracownikId == odejscie.PracownikId);

                var zadaniaDoOdejscia = new List<ZadanieWdrozeniowe>();

                if (odejscie.CzyPrzekierowacPoczte)
                {
                    zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.IT, Tresc = $"Przekierować pocztę na: {odejscie.AdresatPrzekierowania}", CzyWykonane = false });
                }
                else
                {
                    zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.IT, Tresc = "Zablokować konto email i AD", CzyWykonane = false });
                }

                if (!string.IsNullOrEmpty(odejscie.InstrukcjaDlaPlikow))
                {
                    zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.IT, Tresc = $"Pliki: {odejscie.InstrukcjaDlaPlikow}", CzyWykonane = false });
                }

                if (ostatnieWdrozenie != null)
                {
                    if (!string.IsNullOrEmpty(ostatnieWdrozenie.WybranySprzet))
                    {
                        foreach (var item in ostatnieWdrozenie.WybranySprzet.Split(','))
                        {
                            var nazwaSprzetu = item.Trim();
                            bool czyWydano = ostatnieWdrozenie.Zadania.Any(z => z.Dzial == Dzial.Sprzet && z.Tresc.Contains(nazwaSprzetu) && z.CzyWykonane == true);

                            if (czyWydano)
                            {
                                zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.Sprzet, Tresc = $"Odebrać: {nazwaSprzetu}", CzyWykonane = false });
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(ostatnieWdrozenie.DostepDoDyskow))
                    {
                        foreach (var item in ostatnieWdrozenie.DostepDoDyskow.Split(','))
                        {
                            var nazwaDysku = item.Trim();
                            bool czyNadano = ostatnieWdrozenie.Zadania.Any(z => z.Dzial == Dzial.IT && z.Tresc.Contains(nazwaDysku) && z.CzyWykonane == true);

                            if (czyNadano)
                            {
                                zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.IT, Tresc = $"Odebrać dostęp: {nazwaDysku}", CzyWykonane = false });
                            }
                        }
                    }

                    if (ostatnieWdrozenie.WymaganyVPN)
                    {
                        bool czySkonfigurowanoVPN = ostatnieWdrozenie.Zadania.Any(z => z.Tresc.Contains("VPN") && z.CzyWykonane == true);
                        if (czySkonfigurowanoVPN)
                        {
                            zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.IT, Tresc = "Wyłączyć dostęp VPN", CzyWykonane = false });
                        }
                    }
                }
                else
                {
                    zadaniaDoOdejscia.Add(new ZadanieWdrozeniowe { OdejscieId = odejscie.Id, Dzial = Dzial.Sprzet, Tresc = "Zweryfikować sprzęt (brak historii wdrożenia)", CzyWykonane = false });
                }

                _context.ZadaniaWdrozeniowe.AddRange(zadaniaDoOdejscia);
                _context.Notatki.Add(new Notatka { OdejscieId = odejscie.Id, Tresc = $"Rozpoczęto procedurę odejścia. Wygenerowano {zadaniaDoOdejscia.Count} zadań.", Autor = User.Identity.Name, CzyAutomatyczna = true });

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["PracownikId"] = new SelectList(_context.Pracownicy.Where(p => !p.CzyZarchiwizowany), "Id", "ImieNazwisko", odejscie.PracownikId);
            return View(odejscie);
        }

        // POST: Odejscia/WykonajZadanie
        // --- TUTAJ JEST GŁÓWNA ZMIANA BEZPIECZEŃSTWA ---
        [HttpPost]
        public async Task<IActionResult> WykonajZadanie(int zadanieId)
        {
            var zadanie = await _context.ZadaniaWdrozeniowe.FindAsync(zadanieId);

            if (zadanie != null && zadanie.OdejscieId.HasValue)
            {
                // SPRAWDZENIE UPRAWNIEŃ (ROLE)
                bool maUprawnienia = false;

                if (User.IsInRole("Kierownik")) maUprawnienia = true; // Kierownik może wszystko (opcjonalnie)
                else if (zadanie.Dzial == Dzial.IT && User.IsInRole("IT")) maUprawnienia = true;
                else if (zadanie.Dzial == Dzial.HR && User.IsInRole("HR")) maUprawnienia = true;
                else if (zadanie.Dzial == Dzial.Sprzet && User.IsInRole("Sprzet")) maUprawnienia = true;

                if (!maUprawnienia)
                {
                    // Jeśli użytkownik nie ma prawa, dodajemy ostrzeżenie i nie zmieniamy stanu
                    TempData["Error"] = "Brak uprawnień do wykonania tego zadania!";
                    return RedirectToAction(nameof(Details), new { id = zadanie.OdejscieId });
                }

                // Jeśli ma uprawnienia, wykonujemy zmianę
                zadanie.CzyWykonane = !zadanie.CzyWykonane;
                string status = zadanie.CzyWykonane ? "Wykonano" : "Cofnięto";

                _context.Notatki.Add(new Notatka
                {
                    OdejscieId = zadanie.OdejscieId,
                    Tresc = $"{status}: {zadanie.Tresc} ({zadanie.Dzial})",
                    Autor = User.Identity.Name,
                    CzyAutomatyczna = true
                });

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = zadanie.OdejscieId });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DodajNotatke(int id, string tresc)
        {
            if (!string.IsNullOrWhiteSpace(tresc))
            {
                var odejscie = await _context.Odejscia.FindAsync(id);
                if (odejscie != null)
                {
                    if (odejscie.Status == StatusZgloszenia.Nowe) { odejscie.Status = StatusZgloszenia.WToku; _context.Update(odejscie); }
                    _context.Notatki.Add(new Notatka { OdejscieId = id, Tresc = tresc, Autor = User.Identity.Name ?? "User", CzyAutomatyczna = false });
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Zakoncz(int id)
        {
            var odejscie = await _context.Odejscia.Include(o => o.Pracownik).FirstOrDefaultAsync(o => o.Id == id);
            if (odejscie != null && odejscie.Status != StatusZgloszenia.Anulowane)
            {
                odejscie.Status = StatusZgloszenia.Zakonczone;
                if (odejscie.Pracownik != null) { odejscie.Pracownik.CzyZarchiwizowany = true; _context.Update(odejscie.Pracownik); }
                _context.Notatki.Add(new Notatka { OdejscieId = id, Tresc = "Procedura odejścia zakończona. Pracownik archiwizowany.", Autor = User.Identity.Name, CzyAutomatyczna = true });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> Anuluj(int id)
        {
            var odejscie = await _context.Odejscia.FindAsync(id);
            if (odejscie != null)
            {
                odejscie.Status = StatusZgloszenia.Anulowane;
                _context.Notatki.Add(new Notatka { OdejscieId = id, Tresc = "Zgłoszenie ANULOWANE.", Autor = User.Identity.Name, CzyAutomatyczna = true });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}