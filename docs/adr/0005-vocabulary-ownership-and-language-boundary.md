# ADR 0005: Własność słownictwa i granica Languages–Vocabulary

Status: Accepted  
Data: 2026-08-14

## Kontekst

Vocabulary potrzebuje aktywnych języków i integralności referencyjnej do katalogu Languages, ale assembly modułów nie mogą zależeć od siebie. Model musi obsłużyć wieloznaczność, prywatne słowa oraz późniejszą moderację bez upraszczania tłumaczeń do par leksemów.

## Decyzja

- uczoną i dodawaną do słownika jednostką jest `LexemeSense`;
- znaczenia w wielu językach są łączone przez `Concept`, a nie bezpośrednią relacją tłumaczenia 1:1;
- prywatne słowo tworzy prywatny `Concept`, `Lexeme` i `LexemeSense` z tym samym właścicielem; nie dopisuje prywatnej treści do encji katalogowej;
- publiczne odczyty widzą tylko opublikowany katalog, a odczyty użytkownika dodatkowo zasoby z jego `OwnerUserId`;
- Vocabulary odczytuje aktywne języki przez wąski `ILanguageCatalogReader`, bez referencji do assembly Languages;
- cross-schema foreign keys do `languages.Languages` konfiguruje composition root API, który zna oba modele;
- provenance danych katalogowych jest obowiązkowe, a publikacja treści użytkownika będzie późniejszym procesem kopiowania i moderacji;
- prywatne duplikaty oraz wielokrotne dodanie tego samego znaczenia chronią indeksy unikalne i obsługa konfliktów współbieżności.

## Konsekwencje

Model zachowuje wieloznaczność oraz możliwość dodawania języków bez migracji tabel Vocabulary. Granice modułów pozostają egzekwowane testem architektury, choć host ma małą, jawną konfigurację relacji pomiędzy schematami. Prywatna treść nie może przypadkowo pojawić się w publicznym katalogu.

Listy, kategorie, moderacja, relacje semantyczne szersze/węższe oraz spaced repetition nie są częścią tej decyzji implementacyjnej i pozostają późniejszymi przyrostami.
