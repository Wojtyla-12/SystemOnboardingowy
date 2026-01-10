using System.ComponentModel.DataAnnotations;

namespace SystemOnboardingowy.Models
{
    public class Pracownik
    {
        public int Id { get; set; }
        [Display(Name = "Imię")] public string Imie { get; set; }
        [Display(Name = "Nazwisko")] public string Nazwisko { get; set; }
        public string Stanowisko { get; set; }
        public string Email { get; set; }

        public string ImieNazwisko => $"{Imie} {Nazwisko}";
    }
}