//Kierownik: kierownik @firma.pl / Haslo123!(Może dodawać pracowników i wdrożenia)
//IT: it @firma.pl / Haslo123!(Może odhaczać tylko zadania IT)
//HR: hr @firma.pl / Haslo123!(Może odhaczać tylko zadania HR)
//Login: sprzet@firma.pl  Hasło: Haslo123!

Aby odpalić projekt trzeba wykonać migrację bazy 

Add-Migration nowa -Context OnboardingContext
Update-Database -Context OnboardingContext
