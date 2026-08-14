# ADR 0003: Kierunek uwierzytelniania klienta przeglądarkowego

- Status: accepted and implemented
- Data: 2026-08-14

## Kontekst

Blazor WebAssembly jest publicznym, niezaufanym klientem. Przechowywanie długowiecznych bearer tokens w Web Storage zwiększa skutki XSS.

## Decyzja

ASP.NET Core Identity używa zabezpieczonego cookie `HttpOnly`, `Secure` w środowiskach innych niż lokalny Development oraz `SameSite=Lax`. Mutujące endpointy REST wymagają tokenu antiforgery przesyłanego przez PWA w nagłówku `X-XSRF-TOKEN`. Cookie antiforgery jest `HttpOnly`; jawny token pochodzi z dedykowanego endpointu i nie jest utrwalany w Web Storage. Ponieważ middleware tokenowy .NET 10 zapisuje wynik walidacji, lecz nie przerywa automatycznie endpointu JSON, własny middleware fail-closed zwraca `400` przed bindingiem requestu, gdy wynik jest nieważny lub nieobecny. Autoryzacja i wszystkie reguły bezpieczeństwa pozostają po stronie API.

## Konsekwencje

Wdrożenie powinno zachować jeden origin. Lokalne, rozdzielone porty PWA i API wymagają CORS z jawną listą originów oraz credentials. Integracja z przyszłymi klientami natywnymi może później wymagać osobnego przepływu OAuth/OIDC; nie projektujemy go teraz.
