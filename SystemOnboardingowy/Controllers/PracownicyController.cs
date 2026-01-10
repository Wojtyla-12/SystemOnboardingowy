using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemOnboardingowy.Data;
using SystemOnboardingowy.Models;
using Microsoft.AspNetCore.Authorization;

namespace SystemOnboardingowy.Controllers
{
    [Authorize(Roles = "Kierownik")] // DOSTĘP TYLKO DLA KIEROWNIKA
    public class PracownicyController : Controller
    {
        private readonly OnboardingContext _context;

        public PracownicyController(OnboardingContext context)
        {
            _context = context;
        }

        // GET: Pracownicy (Lista)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pracownicy.ToListAsync());
        }

        // GET: Pracownicy/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pracownicy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Imie,Nazwisko,Stanowisko,Email")] Pracownik pracownik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pracownik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pracownik);
        }

        // GET: Pracownicy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pracownik = await _context.Pracownicy.FindAsync(id);
            if (pracownik == null) return NotFound();
            return View(pracownik);
        }

        // POST: Pracownicy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Imie,Nazwisko,Stanowisko,Email")] Pracownik pracownik)
        {
            if (id != pracownik.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(pracownik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pracownik);
        }

        // GET: Pracownicy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var pracownik = await _context.Pracownicy.FirstOrDefaultAsync(m => m.Id == id);
            if (pracownik == null) return NotFound();
            return View(pracownik);
        }

        // POST: Pracownicy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pracownik = await _context.Pracownicy.FindAsync(id);
            if (pracownik != null)
            {
                _context.Pracownicy.Remove(pracownik);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}