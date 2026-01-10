using System.ComponentModel.DataAnnotations;

namespace SystemOnboardingowy.Models
{
    // ENUMERACJA STATUSÓW
    public enum StatusWdrozenia
    {
        Nowe,
        WToku,
        Zakonczone
    }

    // MODEL PROCESU WDROŻENIA
    public class Wdrozenie
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data Rozpoczęcia")]
        public DateTime DataRozpoczecia { get; set; } = DateTime.Now;

        public StatusWdrozenia Status { get; set; } = StatusWdrozenia.Nowe;

        [Display(Name = "Notatki ogólne")]
        public string? Notatki { get; set; } // Notatki widoczne na głównej liście

        public int PracownikId { get; set; }
        public virtual Pracownik? Pracownik { get; set; }

        public virtual List<ZadanieWdrozeniowe>? Zadania { get; set; }
    }
}