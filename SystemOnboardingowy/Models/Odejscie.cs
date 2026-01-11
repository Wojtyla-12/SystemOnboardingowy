using System.ComponentModel.DataAnnotations;

namespace SystemOnboardingowy.Models
{
    public class Odejscie
    {
        public int Id { get; set; }
        public DateTime DataUtworzenia { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime DataOdejscia { get; set; }

        public StatusZgloszenia Status { get; set; } = StatusZgloszenia.Nowe;

        public bool CzyPrzekierowacPoczte { get; set; }
        public string? AdresatPrzekierowania { get; set; }
        public string? InstrukcjaDlaPlikow { get; set; }

        public int PracownikId { get; set; }
        public virtual Pracownik? Pracownik { get; set; }

        public virtual List<Notatka> Notatki { get; set; } = new();

        public virtual List<ZadanieWdrozeniowe> Zadania { get; set; } = new();
    }
}