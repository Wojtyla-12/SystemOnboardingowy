namespace SystemOnboardingowy.Models
{
    public class ZadanieWdrozeniowe
    {
        public int Id { get; set; }
        public string Tresc { get; set; }
        public Dzial Dzial { get; set; }
        public bool CzyWykonane { get; set; }

        public int? WdrozenieId { get; set; }
        public virtual Wdrozenie? Wdrozenie { get; set; }

        public int? OdejscieId { get; set; }
        public virtual Odejscie? Odejscie { get; set; }
    }
}