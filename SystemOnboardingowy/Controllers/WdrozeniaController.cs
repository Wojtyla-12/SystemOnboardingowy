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

        [HttpGet]
        public async Task<IActionResult> PobierzStanowiskoPracownika(int id)
        {
            var pracownik = await _context.Pracownicy.FindAsync(id);
            if (pracownik == null) return NotFound();
            return Json(new { stanowiskoId = (int)pracownik.Stanowisko });
        }

        // GET: Wdrozenia
        public async Task<IActionResult> Index(string sortOrder, string searchString, bool pokazZakonczone = false)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["PokazZakonczone"] = pokazZakonczone;

            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";

            var query = _context.Wdrozenia
                .Include(w => w.Pracownik)
                .Include(w => w.Zadania)
                .AsQueryable();

            if (!pokazZakonczone)
            {
                query = query.Where(w => w.Status != StatusZgloszenia.Zakonczone && w.Status != StatusZgloszenia.Anulowane);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(w => w.Pracownik.Nazwisko.Contains(searchString) || w.Pracownik.Imie.Contains(searchString));
            }

            var lista = await query.ToListAsync();

            Dzial? mojDzial = null;
            if (User.IsInRole("IT")) mojDzial = Dzial.IT;
            if (User.IsInRole("HR")) mojDzial = Dzial.HR;
            if (User.IsInRole("Sprzet")) mojDzial = Dzial.Sprzet;

            switch (sortOrder)
            {
                case "date_desc": lista = lista.OrderByDescending(w => w.DataRozpoczeciaPracy).ToList(); break;
                case "Status": lista = lista.OrderBy(w => w.Status).ToList(); break;
                case "status_desc": lista = lista.OrderByDescending(w => w.Status).ToList(); break;
                case "Name": lista = lista.OrderBy(w => w.Pracownik.Nazwisko).ToList(); break;
                case "name_desc": lista = lista.OrderByDescending(w => w.Pracownik.Nazwisko).ToList(); break;
                default:
                    lista = lista.OrderByDescending(w =>
                        mojDzial.HasValue && w.Zadania.Any(z => z.Dzial == mojDzial && !z.CzyWykonane)
                    )
                    .ThenByDescending(w => w.Status == StatusZgloszenia.Nowe)
                    .ThenBy(w => w.DataRozpoczeciaPracy)
                    .ToList();
                    break;
            }

            return View(lista);
        }

        // GET: Wdrozenia/MojeZadania
        public async Task<IActionResult> MojeZadania()
        {
            Dzial? mojDzial = null;
            if (User.IsInRole("IT")) mojDzial = Dzial.IT;
            else if (User.IsInRole("HR")) mojDzial = Dzial.HR;
            else if (User.IsInRole("Sprzet")) mojDzial = Dzial.Sprzet;

            if (mojDzial == null) return View(new List<ZadanieWdrozeniowe>());

            var zadania = await _context.ZadaniaWdrozeniowe
                .Include(z => z.Wdrozenie).ThenInclude(w => w.Pracownik)
                .Include(z => z.Odejscie).ThenInclude(o => o.Pracownik)
                .Where(z => z.Dzial == mojDzial && !z.CzyWykonane)
                .ToListAsync();

            zadania = zadania.Where(z =>
                (z.Wdrozenie != null && z.Wdrozenie.Status != StatusZgloszenia.Anulowane) ||
                (z.Odejscie != null && z.Odejscie.Status != StatusZgloszenia.Anulowane)
            ).OrderBy(z => z.Wdrozenie?.DataRozpoczeciaPracy ?? z.Odejscie?.DataOdejscia).ToList();

            return View(zadania);
        }

        public async Task<IActionResult> Details(int id)
        {
            var wdrozenie = await _context.Wdrozenia.Include(w => w.Pracownik).Include(w => w.Zadania).Include(w => w.Notatki).FirstOrDefaultAsync(w => w.Id == id);
            if (wdrozenie == null) return NotFound();
            wdrozenie.Notatki = wdrozenie.Notatki.OrderByDescending(n => n.DataUtworzenia).ToList();
            return View(wdrozenie);
        }

        [Authorize(Roles = "Kierownik")]
        public IActionResult Create()
        {
            var model = new WdrozenieCreateViewModel
            {
                PracownicyLista = _context.Pracownicy.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.ImieNazwisko }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Kierownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WdrozenieCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var wdrozenie = new Wdrozenie
                {
                    PracownikId = model.PracownikId,
                    DataRozpoczeciaPracy = model.DataRozpoczeciaPracy,
                    DataUtworzenia = DateTime.Now,
                    Status = StatusZgloszenia.Nowe,
                    Stanowisko = model.Stanowisko,
                    WymaganyVPN = model.WymaganyVPN,
                    AdresyEmail = model.AdresyEmail
                };

                var sprzetList = new List<string>();
                if (model.Sprzet_PC) sprzetList.Add("PC Stacjonarny");
                if (model.Sprzet_Laptop) sprzetList.Add("Laptop");
                if (model.Sprzet_Monitor) sprzetList.Add("Monitor");
                if (model.Sprzet_Myszka) sprzetList.Add("Myszka/Klawiatura");
                if (model.Sprzet_Sluchawki) sprzetList.Add("Słuchawki");
                if (model.Sprzet_Telefon) sprzetList.Add("Telefon");
                wdrozenie.WybranySprzet = string.Join(", ", sprzetList);

                var dyskiList = new List<string>();
                if (model.Dysk_Ksiegowosc) dyskiList.Add("Księgowość");
                if (model.Dysk_Handel) dyskiList.Add("Handel");
                if (model.Dysk_Staz) dyskiList.Add("Staż");
                if (model.Dysk_HR) dyskiList.Add("HR");
                if (model.Dysk_OfficeManager) dyskiList.Add("Office Manager");
                if (model.Dysk_Wspolny) dyskiList.Add("Wspólny");
                wdrozenie.DostepDoDyskow = string.Join(", ", dyskiList);

                _context.Add(wdrozenie);
                await _context.SaveChangesAsync();

                var zadania = new List<ZadanieWdrozeniowe>();
                foreach (var item in sprzetList)
                    zadania.Add(new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, Dzial = Dzial.Sprzet, Tresc = $"Wydać/Zakupić: {item}", CzyWykonane = false });
                if (!string.IsNullOrWhiteSpace(model.AdresyEmail))
                    zadania.Add(new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, Dzial = Dzial.IT, Tresc = $"Utworzyć adresy email: {model.AdresyEmail}", CzyWykonane = false });
                if (model.WymaganyVPN)
                    zadania.Add(new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, Dzial = Dzial.IT, Tresc = "Skonfigurować dostęp VPN", CzyWykonane = false });
                foreach (var dysk in dyskiList)
                    zadania.Add(new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, Dzial = Dzial.IT, Tresc = $"Nadać uprawnienia do dysku: {dysk}", CzyWykonane = false });
                zadania.Add(new ZadanieWdrozeniowe { WdrozenieId = wdrozenie.Id, Dzial = Dzial.HR, Tresc = "Przygotować umowę i szkolenie BHP", CzyWykonane = false });

                _context.ZadaniaWdrozeniowe.AddRange(zadania);
                _context.Notatki.Add(new Notatka { WdrozenieId = wdrozenie.Id, Tresc = $"Utworzono wdrożenie dla stanowiska: {model.Stanowisko}. Start: {wdrozenie.DataRozpoczeciaPracy:d}", Autor = User.Identity.Name, CzyAutomatyczna = true });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            model.PracownicyLista = _context.Pracownicy.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.ImieNazwisko }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DodajNotatke(int id, string tresc)
        {
            if (!string.IsNullOrWhiteSpace(tresc))
            {
                _context.Notatki.Add(new Notatka { WdrozenieId = id, Tresc = tresc, Autor = User.Identity.Name ?? "User", CzyAutomatyczna = false });
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

        [HttpPost]
        public async Task<IActionResult> WykonajZadanie(int zadanieId)
        {
            var zadanie = await _context.ZadaniaWdrozeniowe.FindAsync(zadanieId);

            if (zadanie != null)
            {
                zadanie.CzyWykonane = !zadanie.CzyWykonane;
                string status = zadanie.CzyWykonane ? "Wykonano" : "Cofnięto";

                if (zadanie.WdrozenieId.HasValue)
                {
                    _context.Notatki.Add(new Notatka { WdrozenieId = zadanie.WdrozenieId.Value, Tresc = $"{status}: {zadanie.Tresc} ({zadanie.Dzial})", Autor = User.Identity.Name, CzyAutomatyczna = true });
                    await _context.SaveChangesAsync();

                    var wdrozenie = await _context.Wdrozenia.Include(w => w.Zadania).FirstOrDefaultAsync(w => w.Id == zadanie.WdrozenieId);
                    if (wdrozenie.Zadania.All(z => z.CzyWykonane))
                    {
                        wdrozenie.Status = StatusZgloszenia.Zakonczone;
                        _context.Notatki.Add(new Notatka { WdrozenieId = wdrozenie.Id, Tresc = "Wszystko gotowe. ZAKOŃCZONE.", Autor = "SYSTEM", CzyAutomatyczna = true });
                    }
                    else if (wdrozenie.Status != StatusZgloszenia.WToku) { wdrozenie.Status = StatusZgloszenia.WToku; }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = zadanie.WdrozenieId });
                }
                if (zadanie.OdejscieId.HasValue)
                {
                    _context.Notatki.Add(new Notatka { OdejscieId = zadanie.OdejscieId.Value, Tresc = $"{status}: {zadanie.Tresc} ({zadanie.Dzial})", Autor = User.Identity.Name, CzyAutomatyczna = true });
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "Odejscia", new { id = zadanie.OdejscieId });
                }
            }
            return NotFound();
        }
    }
}