using System.ComponentModel.DataAnnotations;

namespace SystemOnboardingowy.Models
{
    // ENUMERACJA DZIAŁÓW
    public enum Dzial
    {
        HR,
        IT,
        Sprzet
    }

    // MODEL POJEDYNCZEGO ZADANIA
    public class ZadanieWdrozeniowe
    {
        public int Id { get; set; }

        [Required]
        public string TrescZadania { get; set; }

        public Dzial DzialOdpowiedzialny { get; set; }

        public bool CzyWykonane { get; set; }

        [Display(Name = "Uwagi / Komentarz")]
        public string? Komentarz { get; set; } // Pole na uwagi (np. "Brak sprzętu")

        public int WdrozenieId { get; set; }
        public virtual Wdrozenie? Wdrozenie { get; set; }
    }
}