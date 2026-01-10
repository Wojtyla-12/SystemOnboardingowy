namespace SystemOnboardingowy.Models
{
    public class Notatka
    {
        public int Id { get; set; }
        public string Tresc { get; set; }
        public string Autor { get; set; }
        public DateTime DataUtworzenia { get; set; } = DateTime.Now;
        public bool CzyAutomatyczna { get; set; }

        public int? WdrozenieId { get; set; }
        public virtual Wdrozenie? Wdrozenie { get; set; }

        public int? OdejscieId { get; set; }
        public virtual Odejscie? Odejscie { get; set; }
    }
}