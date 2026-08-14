# ADR 0002: Blazor WebAssembly PWA i jeden origin

- Status: accepted
- Data: 2026-08-14

## Kontekst

Pierwszy klient ma działać na desktopie i mobile jako instalowalna PWA oraz korzystać z REST API.

## Decyzja

Klientem jest standalone Blazor WebAssembly PWA. W środowisku wdrożeniowym reverse proxy serwuje statycznego klienta i przekazuje `/api` oraz `/health` do ASP.NET Core pod tym samym originem. Lokalny dev server korzysta z jawnie ograniczonego CORS.

Service worker cache’uje tylko wersjonowaną powłokę statyczną. Odpowiedzi API i health są network-only. MVP nie zapisuje mutacji offline.

## Konsekwencje

Uwierzytelnianie cookie i ochrona CSRF będą prostsze, a klient nie potrzebuje tokenów w local storage. Nginx/reverse proxy staje się elementem ścieżki wdrożeniowej.
