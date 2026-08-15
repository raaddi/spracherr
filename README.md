# Spracher

Produkcyjnie projektowany SaaS do nauki języków, rozwijany jako modularny monolit na .NET 10. **Etapy 0–3 są ukończone**. Vocabulary obejmuje katalog `Concept/LexemeSense/Lexeme`, prywatne słowa, statusy nauki, listy i kategorie. Exercise Engine ma workflow draft → walidacja → publikacja, wersjonowane definicje, przypięte zestawy, serwerowe ocenianie oraz typy `MultipleChoice`, `FillInBlank` i `Translation`; powtórki należą do Etapu 5.

Decyzje architektoniczne znajdują się w [ARCHITECTURE.md](./ARCHITECTURE.md), a kolejność prac w [ROADMAP.md](./ROADMAP.md).

## Wymagania

- .NET SDK 10.0.302 lub zgodny patch wskazany w `global.json`;
- Docker z Docker Compose do pełnego uruchomienia i testów integracyjnych;
- opcjonalnie lokalny PostgreSQL 17, jeśli aplikacja ma działać bez Compose.

## Uruchomienie przez Docker Compose

W PowerShell:

```powershell
Copy-Item .env.example .env
# Ustaw własne POSTGRES_PASSWORD w pliku .env.
docker compose up --build
```

Aplikacja będzie dostępna pod `http://localhost:8080`. Nginx serwuje Blazor PWA i przekazuje `/api` oraz `/health` do backendu, dzięki czemu przeglądarka używa jednego originu.

Zatrzymanie środowiska:

```powershell
docker compose down
```

Powyższa komenda zachowuje wolumen bazy. Usunięcie wolumenu jest osobną, świadomą operacją administracyjną.

## Uruchomienie bez kontenerów aplikacji

Przy działającym PostgreSQL zgodnym z connection stringiem w `appsettings.Development.json`:

```powershell
dotnet tool restore
dotnet restore Spracher.slnx
dotnet run --project src/Spracher.Api -- --migrate
dotnet run --project src/Spracher.Api --launch-profile http
```

W drugim terminalu:

```powershell
dotnet run --project src/Spracher.Web --launch-profile http
```

API działa pod `http://localhost:5180`, a PWA pod `http://localhost:5190`.

## Build i testy

```powershell
dotnet restore Spracher.slnx
dotnet build Spracher.slnx --configuration Release --no-restore
dotnet test Spracher.slnx --configuration Release --no-build
```

Testy integracyjne używają prawdziwego PostgreSQL w Testcontainers. Domyślnie są pomijane, aby zwykły test nie kończył się błędem na komputerze bez Dockera. Aby je uruchomić:

```powershell
$env:RUN_INTEGRATION_TESTS = 'true'
dotnet test tests/Spracher.Api.IntegrationTests
```

CI ustawia tę zmienną i uruchamia komplet testów z Dockerem.

## Konto i profil językowy

Klient PWA obsługuje rejestrację, potwierdzenie e-mail, logowanie, reset hasła, profil i wybór wielu języków. Sesja używa cookie `HttpOnly`; klient nie zapisuje tokenów dostępowych w Web Storage. Każda mutacja pobiera token antiforgery i wysyła go w nagłówku `X-XSRF-TOKEN`.

W `Development` wiadomości e-mail trafiają do pamięci procesu API. Ostatnią wiadomość dla adresu można odczytać wyłącznie w tym środowisku:

```text
GET /api/v1/auth/development-emails/latest?email=learner@example.com
```

To narzędzie służy tylko do lokalnej pracy i testów. Restart API usuwa wiadomości.

## Vocabulary — slice 2A i 2B

Strona `/vocabulary` udostępnia publiczne wyszukiwanie katalogu oraz widok znaczeń, odpowiedników, form, wymowy i przykładów. Po zalogowaniu użytkownik może dodać konkretne znaczenie do swojego słownika, ustawić status `New`, `Learning`, `Learned` lub `Suspended` oraz utworzyć własny prywatny leksem z definicją. Ekran `/vocabulary/manage` służy do organizowania zapisanych znaczeń w prywatne listy i kategorie.

Seed demonstracyjny English–Polish celowo jest mały. Słowo `bank` pokazuje dwa różne znaczenia połączone przez osobne `Concept`: instytucję finansową oraz brzeg rzeki. Masowy import danych nie jest częścią tego przyrostu.

Pomiar wyszukiwania na kontrolowanych 10 000 leksemach oraz zastosowaną optymalizację indeksu opisuje [docs/performance/vocabulary-prefix-search.md](./docs/performance/vocabulary-prefix-search.md).

## Exercise Engine — slice 3A–3C

Strona `/practice` pokazuje opublikowane zestawy oraz pojedyncze ćwiczenia. Zalogowany użytkownik może przejść uporządkowaną sekwencję `MultipleChoice` → `FillInBlank` → `Translation`; każdy typ ma osobny renderer Blazor, ale wspólny lifecycle próby. Pozycja zestawu wskazuje konkretną `ExerciseVersion`, a próba zapisuje również źródłowe `ExerciseSetItemId`. Publiczny payload nie zawiera klucza odpowiedzi ani feedbacku z definicji.

Chronione rolą `Admin` authoring API pozwala tworzyć i publikować zarówno wersje ćwiczeń, jak i zestawy przypiętych wersji. Draft nie trafia do katalogu, a historyczna próba zachowuje wersję, na której została rozpoczęta. Pełny panel autora pozostaje częścią kolejnych etapów Content Engine.

## Migracje

Utworzenie kolejnej migracji:

```powershell
dotnet tool restore
dotnet ef migrations add MigrationName `
  --project src/Spracher.Persistence `
  --startup-project src/Spracher.Api `
  --context SpracherDbContext
```

Zastosowanie migracji jest osobnym krokiem, a nie efektem zwykłego startu API:

```powershell
dotnet run --project src/Spracher.Api -- --migrate
```

W Docker Compose odpowiada za to jednorazowy serwis `migrations`.

## Endpointy techniczne

| Endpoint | Znaczenie |
|---|---|
| `GET /api/v1/system/info` | niesensytywna informacja o API używana przez PWA |
| `GET /health/live` | proces działa; nie odpytuje zależności |
| `GET /health/ready` | API jest gotowe i może połączyć się z PostgreSQL |
| `GET /openapi/v1.json` | kontrakt OpenAPI, tylko w Development |

Najważniejsze endpointy funkcjonalne:

| Endpoint | Znaczenie |
|---|---|
| `GET /api/v1/auth/antiforgery` | token wymagany przez mutujące żądania |
| `POST /api/v1/auth/register` | rejestracja konta SelfLearner |
| `POST /api/v1/auth/login`, `POST /api/v1/auth/logout` | rozpoczęcie i zakończenie sesji cookie |
| `POST /api/v1/auth/confirm-email` | potwierdzenie adresu e-mail |
| `POST /api/v1/auth/forgot-password`, `POST /reset-password` | bezpieczny reset hasła |
| `GET/PUT /api/v1/profile/` | profil bieżącego użytkownika |
| `GET /api/v1/languages` | publiczny katalog aktywnych języków |
| `GET/PUT /api/v1/languages/me` | języki ojczyste i języki nauki użytkownika |
| `GET /api/v1/vocabulary/search` | wyszukiwanie leksemów po języku i znormalizowanej formie |
| `GET /api/v1/vocabulary/lexemes/{id}` | znaczenia, odpowiedniki, formy, wymowa i przykłady |
| `GET /api/v1/vocabulary/me` | słownik bieżącego użytkownika |
| `POST /api/v1/vocabulary/items` | dodanie konkretnego `LexemeSense` do słownika |
| `PUT /api/v1/vocabulary/items/{id}/status` | zmiana statusu nauki |
| `POST /api/v1/vocabulary/private` | utworzenie prywatnego leksemu, znaczenia i pozycji użytkownika |
| `GET/POST /api/v1/vocabulary/me/lists`, `/api/v1/vocabulary/lists` | odczyt i tworzenie prywatnych list słówek |
| `GET/POST/DELETE /api/v1/vocabulary/lists/{id}...` | szczegóły listy oraz dodawanie i usuwanie znaczeń |
| `GET/POST /api/v1/vocabulary/me/categories`, `/api/v1/vocabulary/categories` | odczyt i tworzenie prywatnych kategorii |
| `PUT /api/v1/vocabulary/items/{id}/categories` | zastąpienie przypisań kategorii pozycji użytkownika |

## Konfiguracja

ASP.NET Core używa standardowej hierarchii konfiguracji. Wartości środowiskowe nadpisuje się przez zmienne z separatorem `__`, np.:

```text
ConnectionStrings__Database=Host=postgres;Port=5432;Database=spracher;Username=spracher;Password=...
```

Sekrety nie mogą trafiać do `appsettings.json`, `.env.example` ani repozytorium.

W środowisku innym niż `Development` wysyłka e-mail domyślnie wymaga SMTP. Minimalna konfiguracja produkcyjna obejmuje:

```text
Application__PublicUrl=https://app.example.com
Email__Mode=Smtp
Email__Smtp__Host=smtp.example.com
Email__Smtp__Port=587
Email__Smtp__UserName=...
Email__Smtp__Password=...
Email__Smtp__FromAddress=no-reply@example.com
DataProtection__KeysPath=/persistent/keys
```

Klucze Data Protection muszą być współdzielone i trwałe dla wszystkich replik API. Produkcyjny reverse proxy powinien udostępniać PWA i `/api` pod jednym originem oraz kończyć HTTPS.
