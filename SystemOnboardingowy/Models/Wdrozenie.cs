using System.ComponentModel.DataAnnotations;

namespace SystemOnboardingowy.Models
{
    public enum StatusZgloszenia { Nowe, WToku, Zakonczone, Anulowane }
    public enum Dzial { HR, IT, Sprzet }
    public enum StanowiskoWdrozeniowe
    {
        [Display(Name = "Office Manager")] OfficeManager,
        HR,
        Handlowiec,
        [Display(Name = "Księgowy")] Ksiegowy,
        [Display(Name = "Stażysta")] Stazysta,
        Inne
    }

    public class Wdrozenie
    {
        public int Id { get; set; }
        public DateTime DataUtworzenia { get; set; } = DateTime.Now;

        [Display(Name = "Start pracy")]
        public DateTime DataRozpoczeciaPracy { get; set; }

        public StatusZgloszenia Status { get; set; } = StatusZgloszenia.Nowe;

        public int PracownikId { get; set; }
        public virtual Pracownik? Pracownik { get; set; }

        public StanowiskoWdrozeniowe Stanowisko { get; set; }

        [Display(Name = "Wybrany Sprzęt")]
        public string? WybranySprzet { get; set; }

        [Display(Name = "Dostępy do dysków")]
        public string? DostepDoDyskow { get; set; }

        [Display(Name = "Adresy Email")]
        public string? AdresyEmail { get; set; }

        [Display(Name = "VPN")]
        public bool WymaganyVPN { get; set; }

        public virtual List<ZadanieWdrozeniowe> Zadania { get; set; } = new();
        public virtual List<Notatka> Notatki { get; set; } = new();
    }
}