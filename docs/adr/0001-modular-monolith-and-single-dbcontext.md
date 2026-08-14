# ADR 0001: Modularny monolit i jeden DbContext

- Status: accepted
- Data: 2026-08-14

## Kontekst

Produkt ma wiele przyszłych domen, ale na początku wymaga szybkich, transakcyjnych przyrostów i prostego wdrożenia.

## Decyzja

Backend jest jednym procesem ASP.NET Core. Moduły mają osobne assemblies i docelowo osobne schematy PostgreSQL. Jeden techniczny `SpracherDbContext` oraz jeden strumień migracji upraszczają transakcje i testy. Persistence nie referencjonuje modułów; moduły rejestrują własne konfiguratory modelu w composition root.

## Konsekwencje

Granice muszą być sprawdzane przez code review i architecture tests. Rozdzielenie kontekstu lub procesu jest możliwe później, ale wymaga realnego uzasadnienia, nie hipotetycznej potrzeby skalowania.
