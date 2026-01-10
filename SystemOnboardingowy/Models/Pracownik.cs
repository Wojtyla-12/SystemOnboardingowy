using System.ComponentModel.DataAnnotations;

namespace SystemOnboardingowy.Models
{
    // MODEL DANYCH PRACOWNIKA
    public class Pracownik
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string Nazwisko { get; set; }

        [Required]
        public string Stanowisko { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Relacja: Pracownik może mieć historię wdrożeń
        public virtual ICollection<Wdrozenie>? Wdrozenia { get; set; }
    }
}