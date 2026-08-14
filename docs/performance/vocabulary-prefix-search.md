# Vocabulary prefix search — baseline

Data pomiaru: 2026-08-14

Środowisko: lokalny PostgreSQL 17, baza deweloperska, 10 000 kontrolowanych leksemów dodanych wewnątrz transakcji zakończonej `ROLLBACK`.

## Cel

Sprawdzić plan zapytania używanego przez publiczne wyszukiwanie słownictwa: filtr języka, widoczności i statusu publikacji oraz prefiks znormalizowanej lemmy (`LIKE 'prefix%'`). Pomiar obejmuje osobno `COUNT(*)` i pobranie pierwszych 20 wyników z wyliczeniem liczby znaczeń.

## Wynik

| Wariant | Plan dla leksemów | `COUNT(*)` | Pierwsza strona |
|---|---|---:|---:|
| zwykły indeks B-tree `(LanguageId, NormalizedLemma)` | `Seq Scan`, 9 907 odrzuconych wierszy | 2,915 ms | 3,425 ms |
| indeks z `text_pattern_ops` dla `NormalizedLemma` | `Index Scan`, 6 trafień bufora | 0,158 ms | 0,505 ms |

Pomiar wykazał, że zwykły indeks B-tree nie obsługiwał efektywnie prefiksowego `LIKE` przy bieżącej kolacji PostgreSQL. Migracja `OptimizeVocabularyPrefixSearch` ustawia klasy operatorów `uuid_ops` i `text_pattern_ops`. Po zmianie oba zapytania użyły `IX_Lexemes_LanguageId_NormalizedLemma`.

Wyniki są lokalnym smoke testem planu, nie deklaracją produkcyjnego SLA. Przed masowym importem trzeba powtórzyć pomiar na danych o realistycznym rozkładzie języków, długości lemm i liczbie znaczeń.

## Zasady powtarzania pomiaru

- dane kontrolne powstają w jawnej transakcji i są zawsze wycofywane;
- po insercie wykonywane jest `ANALYZE vocabulary."Lexemes"`;
- akceptowany plan dla selektywnego prefiksu musi zawierać `Index Scan` lub `Bitmap Index Scan` na `IX_Lexemes_LanguageId_NormalizedLemma`;
- nie ustawiamy sztywnego progu czasu w CI, ponieważ byłby zależny od hosta; kontrolujemy kształt planu i regresje indeksów.
