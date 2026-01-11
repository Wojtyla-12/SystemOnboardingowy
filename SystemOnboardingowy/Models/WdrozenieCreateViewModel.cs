using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SystemOnboardingowy.Models
{
    public class WdrozenieCreateViewModel
    {
        [Display(Name = "Pracownik")]
        public int PracownikId { get; set; }

        [Display(Name = "Data i godzina rozpoczęcia pracy")]
        public DateTime DataRozpoczeciaPracy { get; set; } = DateTime.Today.AddDays(7).AddHours(8);

        [Display(Name = "Stanowisko")]
        public StanowiskoWdrozeniowe Stanowisko { get; set; }

        [Display(Name = "Komputer Stacjonarny")] public bool Sprzet_PC { get; set; }
        [Display(Name = "Laptop")] public bool Sprzet_Laptop { get; set; }
        [Display(Name = "Monitor")] public bool Sprzet_Monitor { get; set; }
        [Display(Name = "Myszka / Klawiatura")] public bool Sprzet_Myszka { get; set; }
        [Display(Name = "Słuchawki")] public bool Sprzet_Sluchawki { get; set; }
        [Display(Name = "Telefon służbowy")] public bool Sprzet_Telefon { get; set; }

        [Display(Name = "Adresy Email")] public string? AdresyEmail { get; set; }
        [Display(Name = "VPN")] public bool WymaganyVPN { get; set; }

        [Display(Name = "Dysk Księgowość")] public bool Dysk_Ksiegowosc { get; set; }
        [Display(Name = "Dysk Handel")] public bool Dysk_Handel { get; set; }
        [Display(Name = "Dysk Staż")] public bool Dysk_Staz { get; set; }
        [Display(Name = "Dysk HR")] public bool Dysk_HR { get; set; }
        [Display(Name = "Dysk Office Manager")] public bool Dysk_OfficeManager { get; set; }
        [Display(Name = "Dysk Wspólny")] public bool Dysk_Wspolny { get; set; }

        public IEnumerable<SelectListItem>? PracownicyLista { get; set; }
    }
}