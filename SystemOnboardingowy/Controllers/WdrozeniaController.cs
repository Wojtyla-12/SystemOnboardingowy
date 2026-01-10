using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Data;
using SystemOnboardingowy.Models;
using Microsoft.AspNetCore.Authorization;

namespace SystemOnboardingowy.Controllers
{
    [Authorize] // Wymaga zalogowania
    public class WdrozeniaController : Controller
    {
        private readonly OnboardingContext _context;

        public WdrozeniaController(OnboardingContext context)
        {
            _context = context;
        }

        // --- 1. LISTA WDROŻEŃ (Z FILTROWANIEM I INTELIGENTNYM SORTOWANIEM) ---
        public async Task<IActionResult> Index(string searchString, StatusWdrozenia? statusFilter)
        {
            // 1. Pobieranie danych (Eager Loading)
            var query = _context.Wdrozenia
                .Include(w => w.Pracownik)
                .Include(w => w.Zadania)
                .AsQueryable();

            // 2. Filtrowanie po nazwisku (Search)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Pracownik.Nazwisko.Contains(searchString)
                                      || s.Pracownik.Imie.Contains(searchString));
            }

            // 3. Filtrowanie po statusie
            if (statusFilter.HasValue)
            {
                query = query.Where(w => w.Status == statusFilter.Value);
            }

            // Pobieramy listę do pamięci, aby wykonać skomplikowane sortowanie zależne od ról
            var listaWdrozen = await query.ToListAsync();

            // 4. INTELIGENTNE SORTOWANIE
            // Logika: Jeśli wdrożenie ma niewykonane zadania dla MOJEGO działu -> Idzie na samą górę.

            if (User.IsInRole("IT"))
            {
                listaWdrozen = listaWdrozen
                    .OrderByDescending(w => w.Zadania.Any(z => z.DzialOdpowiedzialny == Dzial.IT && !z.CzyWykonane))
                    .ThenBy(w => w.DataRozpoczecia)
                    .ToList();
            }
            else if (User.IsInRole("HR"))
            {
                listaWdrozen = listaWdrozen
                    .OrderByDescending(w => w.Zadania.Any(z => z.DzialOdpowiedzialny == Dzial.HR && !z.CzyWykonane))
                    .ThenBy(w => w.DataRozpoczecia)
                    .ToList();
            }
            else if (User.IsInRole("Sprzet"))
            {
                listaWdrozen = listaWdrozen
                    .OrderByDescending(w => w.Zadania.Any(z => z.DzialOdpowiedzialny == Dzial.Sprzet && !z.CzyWykonane))
                    .ThenBy(w => w.DataRozpoczecia)
                    .ToList();
            }
            else
            {
                // Domyślne sortowanie (np. dla Kierownika): Zakończone na dole
                listaWdrozen = listaWdrozen
                    .OrderBy(w => w.Status == StatusWdrozenia.Zakonczone)
                    .ThenByDescending(w => w.DataRozpoczecia)
                    .ToList();
            }

            // Przekazanie filtrów do widoku
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            return View(listaWdrozen);
        }

        // --- 2. DASHBOARD: MOJE ZADANIA ---
        public async Task<IActionResult> MojeZadania()
        {
            var zadaniaQuery = _context.Zadania
                .Include(z => z.Wdrozenie)
                .ThenInclude(w => w.Pracownik)
                .Where(z => !z.CzyWykonane); // Tylko niewykonane

            if (User.IsInRole("IT"))
                zadaniaQuery = zadaniaQuery.Where(z => z.DzialOdpowiedzialny == Dzial.IT);
            else if (User.IsInRole("HR"))
                zadaniaQuery = zadaniaQuery.Where(z => z.DzialOdpowiedzialny == Dzial.HR);
            else if (User.IsInRole("Sprzet"))
                zadaniaQuery = zadaniaQuery.Where(z => z.DzialOdpowiedzialny == Dzial.Sprzet);

            return View(await zadaniaQuery.OrderBy(z => z.Wdrozenie.DataRozpoczecia).ToListAsync());
        }

        // --- 3. SZCZEGÓŁY WDROŻENIA ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var wdrozenie = await _context.Wdrozenia
                .Include(w => w.Pracownik)
                .Include(w => w.Zadania)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (wdrozenie == null) return NotFound();

            return View(wdrozenie);
        }

        // --- 4. TWORZENIE (TYLKO KIEROWNIK) ---
        [Authorize(Roles = "Kierownik")]
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownicy, "Id", "Nazwisko");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PracownikId,DataRozpoczecia,Notatki")] Wdrozenie wdrozenie)
        {
            if (ModelState.IsValid)
            {
                wdrozenie.Status = StatusWdrozenia.Nowe;
                _context.Add(wdrozenie);
                await _context.SaveChangesAsync();

                // Generowanie zadań startowych
                var zadania = new List<ZadanieWdrozeniowe>
                {
                    new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, DzialOdpowiedzialny = Dzial.IT, TrescZadania = "Założenie konta domenowego i e-mail", CzyWykonane = false },
                    new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, DzialOdpowiedzialny = Dzial.IT, TrescZadania = "Dostęp do VPN", CzyWykonane = false },
                    new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, DzialOdpowiedzialny = Dzial.Sprzet, TrescZadania = "Wydanie laptopa i telefonu", CzyWykonane = false },
                    new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, DzialOdpowiedzialny = Dzial.HR, TrescZadania = "Umowa o pracę", CzyWykonane = false },
                    new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, DzialOdpowiedzialny = Dzial.HR, TrescZadania = "Szkolenie BHP", CzyWykonane = false }
                };
                _context.Zadania.AddRange(zadania);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownicy, "Id", "Nazwisko", wdrozenie.PracownikId);
            return View(wdrozenie);
        }

        // --- 5. DODAWANIE NIESTANDARDOWEGO ZADANIA ---
        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> DodajZadanie(int wdrozenieId, string tresc, Dzial dzial)
        {
            if (!string.IsNullOrEmpty(tresc))
            {
                var noweZadanie = new ZadanieWdrozeniowe
                {
                    WdrozenieId = wdrozenieId,
                    TrescZadania = tresc,
                    DzialOdpowiedzialny = dzial,
                    CzyWykonane = false
                };

                _context.Zadania.Add(noweZadanie);
                await _context.SaveChangesAsync();

                // Przywracamy status "W Toku" jeśli wdrożenie było zakończone
                var wdrozenie = await _context.Wdrozenia.Include(w => w.Zadania).FirstOrDefaultAsync(w => w.Id == wdrozenieId);
                if (wdrozenie != null)
                {
                    wdrozenie.Status = StatusWdrozenia.WToku;
                    _context.Update(wdrozenie);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Details), new { id = wdrozenieId });
        }

        // --- 6. EDYCJA (TYLKO KIEROWNIK) ---
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var wdrozenie = await _context.Wdrozenia.FindAsync(id);
            if (wdrozenie == null) return NotFound();
            ViewData["PracownikId"] = new SelectList(_context.Pracownicy, "Id", "Nazwisko", wdrozenie.PracownikId);
            return View(wdrozenie);
        }

        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PracownikId,DataRozpoczecia,Notatki")] Wdrozenie wdrozenie)
        {
            if (id != wdrozenie.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var original = await _context.Wdrozenia.FindAsync(id);
                if (original != null)
                {
                    original.DataRozpoczecia = wdrozenie.DataRozpoczecia;
                    original.Notatki = wdrozenie.Notatki;
                    original.PracownikId = wdrozenie.PracownikId;
                    _context.Update(original);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownicy, "Id", "Nazwisko", wdrozenie.PracownikId);
            return View(wdrozenie);
        }

        // --- 7. USUWANIE (TYLKO KIEROWNIK) ---
        [Authorize(Roles = "Kierownik")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var wdrozenie = await _context.Wdrozenia
                .Include(w => w.Pracownik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (wdrozenie == null) return NotFound();
            return View(wdrozenie);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Kierownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wdrozenie = await _context.Wdrozenia.Include(w => w.Zadania).FirstOrDefaultAsync(w => w.Id == id);
            if (wdrozenie != null)
            {
                if (wdrozenie.Zadania != null) _context.Zadania.RemoveRange(wdrozenie.Zadania);
                _context.Wdrozenia.Remove(wdrozenie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 8. API I POMOCNICZE ---

        [HttpPost]
        public async Task<IActionResult> OznaczZadanie(int id, bool czyWykonane, string komentarz)
        {
            var zadanie = await _context.Zadania.FindAsync(id);
            if (zadanie == null) return NotFound();

            zadanie.CzyWykonane = czyWykonane;
            if (komentarz != null) zadanie.Komentarz = komentarz;
            _context.Update(zadanie);
            await _context.SaveChangesAsync();

            // Przeliczanie statusu
            var wdrozenie = await _context.Wdrozenia.Include(w => w.Zadania).FirstOrDefaultAsync(w => w.Id == zadanie.WdrozenieId);
            if (wdrozenie != null)
            {
                if (wdrozenie.Zadania.All(z => z.CzyWykonane)) wdrozenie.Status = StatusWdrozenia.Zakonczone;
                else if (wdrozenie.Zadania.Any(z => z.CzyWykonane)) wdrozenie.Status = StatusWdrozenia.WToku;
                else wdrozenie.Status = StatusWdrozenia.Nowe;
                _context.Update(wdrozenie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = zadanie.WdrozenieId });
        }

        [HttpPost]
        public async Task<IActionResult> ZapiszNotatkeWdrozenia(int id, string notatka)
        {
            var wdrozenie = await _context.Wdrozenia.FindAsync(id);
            if (wdrozenie != null)
            {
                wdrozenie.Notatki = notatka;
                _context.Update(wdrozenie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ZapiszKomentarzZadania(int id, string komentarz)
        {
            var zadanie = await _context.Zadania.FindAsync(id);
            if (zadanie != null)
            {
                zadanie.Komentarz = komentarz;
                _context.Update(zadanie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = zadanie.WdrozenieId });
            }
            return NotFound();
        }
    }
}