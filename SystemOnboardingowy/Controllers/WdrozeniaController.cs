using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Data;
using SystemOnboardingowy.Models;

namespace SystemOnboardingowy.Controllers
{
    [Authorize]
    public class WdrozeniaController : Controller
    {
        private readonly OnboardingContext _context;

        public WdrozeniaController(OnboardingContext context) { _context = context; }

        public async Task<IActionResult> Index(bool pokazZakonczone = false)
        {
            var query = _context.Wdrozenia
                .Include(w => w.Pracownik)
                .Include(w => w.Zadania)
                .AsQueryable();

            if (!pokazZakonczone)
            {
                query = query.Where(w => w.Status != StatusZgloszenia.Zakonczone && w.Status != StatusZgloszenia.Anulowane);
            }

            var lista = await query.ToListAsync();

            Dzial? mojDzial = null;
            if (User.IsInRole("IT")) mojDzial = Dzial.IT;
            if (User.IsInRole("HR")) mojDzial = Dzial.HR;
            if (User.IsInRole("Sprzet")) mojDzial = Dzial.Sprzet;

            lista = lista.OrderByDescending(w =>
                mojDzial.HasValue && w.Zadania.Any(z => z.Dzial == mojDzial && !z.CzyWykonane)
            )
            .ThenByDescending(w => w.Status == StatusZgloszenia.Nowe)
            .ThenBy(w => w.DataRozpoczeciaPracy)
            .ToList();

            ViewData["PokazZakonczone"] = pokazZakonczone;
            return View(lista);
        }

        public async Task<IActionResult> Details(int id)
        {
            var wdrozenie = await _context.Wdrozenia
                .Include(w => w.Pracownik)
                .Include(w => w.Zadania)
                .Include(w => w.Notatki)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wdrozenie == null) return NotFound();
            wdrozenie.Notatki = wdrozenie.Notatki.OrderByDescending(n => n.DataUtworzenia).ToList();

            return View(wdrozenie);
        }

        [HttpPost]
        public async Task<IActionResult> DodajNotatke(int id, string tresc)
        {
            if (!string.IsNullOrWhiteSpace(tresc))
            {
                _context.Notatki.Add(new Notatka
                {
                    WdrozenieId = id,
                    Tresc = tresc,
                    Autor = User.Identity.Name ?? "User",
                    CzyAutomatyczna = false
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> Anuluj(int id)
        {
            var wdrozenie = await _context.Wdrozenia.FindAsync(id);
            if (wdrozenie != null)
            {
                wdrozenie.Status = StatusZgloszenia.Anulowane;
                _context.Notatki.Add(new Notatka { WdrozenieId = id, Tresc = "Zgłoszenie ANULOWANE.", Autor = User.Identity.Name, CzyAutomatyczna = true });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Kierownik")]
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownicy, "Id", "ImieNazwisko");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> Create(Wdrozenie wdrozenie)
        {
            wdrozenie.DataUtworzenia = DateTime.Now;
            wdrozenie.Status = StatusZgloszenia.Nowe;
            _context.Add(wdrozenie);
            await _context.SaveChangesAsync();

            var zadania = new List<ZadanieWdrozeniowe> {
                new() { WdrozenieId = wdrozenie.Id, Dzial = Dzial.IT, Tresc = "Konto AD, Email, VPN", CzyWykonane = false },
                new() { WdrozenieId = wdrozenie.Id, Dzial = Dzial.Sprzet, Tresc = "Laptop, Monitor, Telefon", CzyWykonane = false },
                new() { WdrozenieId = wdrozenie.Id, Dzial = Dzial.HR, Tresc = "Umowa i BHP", CzyWykonane = false }
            };
            _context.ZadaniaWdrozeniowe.AddRange(zadania);
            _context.Notatki.Add(new Notatka { WdrozenieId = wdrozenie.Id, Tresc = $"Utworzono. Start: {wdrozenie.DataRozpoczeciaPracy:d}", Autor = User.Identity.Name, CzyAutomatyczna = true });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> WykonajZadanie(int zadanieId)
        {
            var zadanie = await _context.ZadaniaWdrozeniowe.FindAsync(zadanieId);
            if (zadanie != null)
            {
                zadanie.CzyWykonane = !zadanie.CzyWykonane;
                string status = zadanie.CzyWykonane ? "Wykonano" : "Cofnięto";
                _context.Notatki.Add(new Notatka
                {
                    WdrozenieId = zadanie.WdrozenieId,
                    Tresc = $"{status}: {zadanie.Tresc} ({zadanie.Dzial})",
                    Autor = User.Identity.Name,
                    CzyAutomatyczna = true
                });
                await _context.SaveChangesAsync();

                var wdrozenie = await _context.Wdrozenia.Include(w => w.Zadania).FirstOrDefaultAsync(w => w.Id == zadanie.WdrozenieId);
                if (wdrozenie.Zadania.All(z => z.CzyWykonane))
                {
                    wdrozenie.Status = StatusZgloszenia.Zakonczone;
                    _context.Notatki.Add(new Notatka { WdrozenieId = wdrozenie.Id, Tresc = "Wszystko gotowe. ZAKOŃCZONE.", Autor = "SYSTEM", CzyAutomatyczna = true });
                }
                else if (wdrozenie.Status != StatusZgloszenia.WToku)
                {
                    wdrozenie.Status = StatusZgloszenia.WToku;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = zadanie.WdrozenieId });
            }
            return NotFound();
        }
    }
}