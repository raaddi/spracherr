# ADR 0004: Minimalny projekt modelu storage Identity

- Status: accepted
- Data: 2026-08-14

## Kontekst

Jeden `SpracherDbContext` ma przechowywać tabele ASP.NET Core Identity i dane modułów. Poprawne mapowanie użytkowników i ról wymaga dziedziczenia z generycznego `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`. Umieszczenie tych typów wyłącznie w IdentityAccess wymusiłoby zależność Persistence od modułu biznesowego albo cykl referencji.

## Decyzja

Powstaje mały projekt `Spracher.IdentityModel` zawierający wyłącznie typy storage `ApplicationUser`, `ApplicationRole`, status konta i stałe seedowanych ról. Persistence może go referencjonować, natomiast endpointy, konfiguracja bezpieczeństwa, e-mail i use cases pozostają w `Spracher.Modules.IdentityAccess`.

## Konsekwencje

Granica jest celowym technicznym seamem, a nie nowym modułem domenowym. IdentityModel nie może przejmować workflow konta ani zależeć od Persistence lub modułów. Dzięki temu zachowujemy jeden kontekst, liniowe migracje i testowalną regułę braku zależności Persistence od modułów biznesowych.
