# Spracher — roadmapa implementacji

Roadmapa prowadzi przez pionowe, działające przyrosty. Każdy etap kończy się demonstracyjnym przepływem, testami i decyzją, czy kolejny zakres nadal odpowiada potrzebom produktu. Kolejność może się zmienić po walidacji produktu, ale zależności techniczne powinny zostać zachowane.

## Założenie zakresu MVP

Pierwsze wydanie powinno udowodnić główną pętlę self-learning:

> użytkownik zakłada konto → wybiera język → otwiera opublikowany temat → wykonuje ćwiczenia → dodaje słownictwo → robi powtórkę → widzi postęp

Teacher Mode jest kolejnym przyrostem MVP, ale nie powinien blokować pierwszego wydania self-learning. Live Classroom, szkoły i billing nie należą do MVP.

## Etap 0 — fundament techniczny

Status: **ukończony 2026-08-14**. Build, formatowanie, smoke test API i testy bez kontenerów zostały zweryfikowane lokalnie. Testy PostgreSQL/Testcontainers oraz obrazy Compose są weryfikowane w CI, ponieważ lokalne środowisko wykonawcze nie udostępnia Dockera.

Cel: aplikacja uruchamia się lokalnie i ma zweryfikowaną ścieżkę wdrożenia bez funkcji biznesowych.

Zakres:

- scaffold solution zgodny z `ARCHITECTURE.md`, ale tylko dla aktualnie potrzebnych projektów;
- `Spracher.Api`, `Spracher.Web`, Contracts, BuildingBlocks, Persistence, IdentityAccess i Languages;
- PostgreSQL, EF Core, pierwsza migracja i automatyczne stosowanie migracji jako osobny krok wdrożenia;
- Dockerfile oraz Docker Compose dla API, web i PostgreSQL;
- konfiguracja przez environment variables i bezpieczny `.env.example`;
- health checks, strukturalne logowanie i globalne `ProblemDetails`;
- bazowy pipeline CI: restore, build, format/analyzers, unit tests, integration tests;
- test architektoniczny zależności modułów;
- bazowy responsive shell PWA, manifest i bezpieczna strategia cache assetów;
- ADR dotyczące hostowania, uwierzytelniania oraz jednego `DbContext`.

Kryterium ukończenia:

- czysty checkout uruchamia się jedną opisaną komendą;
- API łączy się z PostgreSQL i raportuje readiness;
- klient PWA komunikuje się z wersjonowanym endpointem API;
- CI odtwarza build i testy bez lokalnych zależności dewelopera.

Poza zakresem: funkcje domenowe, SignalR, Kubernetes, monitoring produkcyjny klasy enterprise.

## Etap 1 — Identity i katalog języków

Status: **ukończony 2026-08-14**. Zaimplementowano sesję cookie z ochroną antiforgery, pełne podstawowe lifecycle konta, pięć seedowanych języków, profile językowe, UI PWA i test pionowego przepływu na PostgreSQL/Testcontainers. Lokalnie zweryfikowano build, formatowanie i unit/architecture tests; uruchomienie testów kontenerowych pozostaje odpowiedzialnością CI na maszynie z Dockerem.

Cel: bezpieczne konto oraz wybór wielu języków nauki.

Zakres:

- ASP.NET Core Identity: rejestracja, logowanie, wylogowanie, potwierdzenie e-mail i reset hasła;
- role wielowartościowe i początkowe policies;
- cookie + antiforgery dla Blazor WASM; brak tokenów w local storage;
- profil użytkownika i preferencje strefy czasowej;
- encje `Language` i `UserLanguageProfile`;
- seed danych Polish, English, German, Spanish i French jako danych w bazie;
- wybór języka ojczystego i jednego lub wielu języków nauki;
- podstawowa strona profilu;
- rate limiting i testy negatywne autoryzacji.

Kryterium ukończenia:

- użytkownik może bezpiecznie zarządzać sesją i swoimi językami;
- dodanie szóstego języka wymaga wyłącznie danych, nie migracji ani zmiany modelu;
- krytyczne endpointy mają testy integracyjne z realnym PostgreSQL.

## Etap 2 — Vocabulary: pierwszy pionowy slice

Status: **w toku — slice 2A i funkcjonalny slice 2B ukończone 2026-08-14**. Zaimplementowano model i schemat `Concept/LexemeSense/Lexeme`, mały seed English–Polish, wyszukiwanie i szczegóły, `UserVocabularyItem`, statusy, prywatny leksem, listy słówek, kategorie M:N, endpointy REST, responsywne widoki PWA, migracje oraz testy domenowe i integracyjne scenariusze własności. Pełny przepływ został dodatkowo sprawdzony lokalnie na PostgreSQL 17.

Do formalnego zamknięcia całego Etapu 2 pozostają podstawowe pomiary zapytań na większym, kontrolowanym zbiorze. Testy Testcontainers pozostają obowiązkowe w CI na maszynie z Dockerem. Powtórki i ich harmonogram pozostają w Etapie 5.

Cel: potwierdzić model `Concept -> LexemeSense -> Lexeme` oraz prywatne słownictwo użytkownika.

Zakres:

- Concept, Lexeme, LexemeSense, WordForm, Pronunciation, LexemeFeature i przykłady;
- wyszukiwanie po języku i znormalizowanej lemma;
- mały, ręcznie zweryfikowany zestaw danych English–Polish; nie masowy import;
- dodanie wybranego znaczenia do `UserVocabularyItem`;
- prywatny Lexeme/Concept utworzony przez użytkownika;
- listy słówek i kategorie; statusy New/Learning/Learned/Suspended są już dostępne;
- prosta strona szczegółów leksemu i listy użytkownika;
- provenance/import id dla danych katalogowych oraz strategia wykrywania duplikatów;
- unit testy wieloznaczności, własności i przejść statusów;
- paginacja, właściwe indeksy i podstawowe pomiary zapytań.

Kryterium ukończenia:

- jeden leksem może należeć do wielu konceptów, a koncept zawiera odpowiedniki w wielu językach;
- własne słowa są niewidoczne dla innych użytkowników;
- użytkownik tworzy listę, kategoryzuje znaczenie i oznacza je jako nauczone;
- model nie zawiera kolumn zależnych od konkretnych języków.

## Etap 3 — Exercise Engine

Cel: jeden wspólny lifecycle ćwiczeń sprawdzony na kilku rzeczywiście różnych typach.

Zakres:

- ExerciseDefinition, niemutowalne ExerciseVersion, ExerciseSet i próby;
- rejestr handlerów, wersjonowanie payloadów i walidacja definicji;
- typy `MultipleChoice`, `FillInBlank` oraz `Translation`;
- renderery Blazor, obsługa klawiatury/dotyku i dostępność;
- przygotowanie DTO bez ujawnienia poprawnych odpowiedzi;
- deterministyczne ocenianie i bezpieczny feedback;
- zapis dokładnej wersji ćwiczenia oraz odpowiedzi;
- unit testy handlerów i kontraktów serializacji;
- integration test: publikacja definicji → rozwiązanie → wynik.

Kryterium ukończenia:

- nowy typ można dodać przez handler, schemat payloadu i komponent bez zmiany tabel prób;
- starsza opublikowana wersja nadal daje odtwarzalny wynik po utworzeniu nowej;
- manipulacja odpowiedzią po stronie klienta nie pozwala przyznać wyniku po stronie serwera.

Poza zakresem: MatchPairs, Listening, Speaking, SentenceOrdering, automatyczna ocena długiego tekstu i AI.

## Etap 4 — Content Engine i pierwszy English Grammar

Cel: opublikować pierwszy mały kurs bez kodowania gramatyki jako stron.

Zakres:

- Course, CourseLevel, Unit, Topic i TopicRevision;
- bloki `RichText`, `Callout`, `Example`, `Table` oraz referencje do mediów;
- TopicVocabulary oraz wersjonowane referencje do ExerciseSet;
- workflow Draft → validation → Published;
- prosty panel/admin flow dla uprawnionego autora; nie pełny WYSIWYG;
- sanitizacja i bezpieczne renderowanie treści;
- English Grammar: mały zakres A0/A1, np. 1–2 units i kilka topics;
- testy migracji/kompatybilności schema versions bloków;
- preview wersji roboczej i niemutowalność wersji opublikowanej.

Kryterium ukończenia:

- administrator tworzy temat z teorią, przykładami, słownictwem i ćwiczeniami bez zmiany kodu strony;
- użytkownik widzi opublikowaną wersję, ale nie draft;
- edycja tworzy nową rewizję i nie zmienia historycznych prób.

## Etap 5 — Learning loop, Daily Lesson i proste powtórki

Cel: ukończyć pierwszy self-learning MVP.

Zakres:

- CourseEnrollment, TopicProgress i LearningSession;
- deterministyczny generator Daily Lesson;
- flashcards i vocabulary training jako przepływy korzystające z Vocabulary i Exercises;
- prosty `VocabularyReviewState` oraz `VocabularyReviewLog`;
- na początek jawny, prosty harmonogram powtórek z wersjonowanymi regułami;
- dashboard bieżącego kursu, dzisiejszej lekcji i postępu;
- wznowienie przerwanej sesji i idempotentne zapisanie wyniku;
- podstawowe wskaźniki produktu: rozpoczęcie/ukończenie lekcji i retencja bez logowania prywatnej treści;
- test e2e głównej pętli self-learning na desktopie i małym viewportcie.

Kryterium ukończenia Self-Learning MVP:

- pełna ścieżka opisana na początku roadmapy działa na komputerze i urządzeniu mobilnym/PWA;
- utrata połączenia nie niszczy zakończonej, zatwierdzonej pracy, a mutacje offline są jasno zablokowane;
- postęp i historia odpowiedzi są spójne po retry;
- istnieje przynajmniej jeden mały, jakościowy fragment English Grammar.

## Etap 6 — minimalna gamifikacja

Cel: dodać motywację dopiero po ustabilizowaniu zdarzeń nauki.

Zakres:

- idempotentny `XpLedgerEntry`;
- wersjonowana tabela progów Account Level 0–99;
- profil z sumą XP i poziomem konta;
- streak wyliczany w strefie czasowej użytkownika;
- kilka statycznie zdefiniowanych achievements i badges;
- reguły antyduplikacyjne oraz testy retry.

Kryterium ukończenia:

- to samo zdarzenie nauki nie może naliczyć XP dwa razy;
- zmiana CEFR nie zmienia Account Level i odwrotnie;
- korekta ledgeru pozostawia audytowalny ślad.

Poza zakresem: rankingi globalne, daily/weekly quests i rozbudowana ekonomia punktów.

## Etap 7 — Teacher Mode i Assessments MVP

Cel: nauczyciel tworzy klasę, przydziela materiał i ocenia test.

Kolejność wewnętrzna:

1. `Classroom`, członkostwo, zaproszenia i resource authorization.
2. Assignment materiału lub listy słownictwa z terminem.
3. Test, TestVersion i pytania wykorzystujące przypięte ExerciseVersion.
4. TestAttempt, StudentAnswer, automatyczna i ręczna ocena.
5. Widok wyników oraz podstawowy postęp ucznia.

Zakres bezpieczeństwa:

- audyt zmiany ocen i członkostwa;
- minimalizacja danych widocznych nauczycielowi;
- analiza prywatności i wymagań dotyczących niepełnoletnich przed publicznym rolloutem;
- testy dostępu nauczyciela do cudzej klasy oraz ucznia do cudzych wyników.

Kryterium ukończenia Teacher MVP:

- nauczyciel zaprasza ucznia, przydziela materiał i wersjonowany test;
- uczeń rozwiązuje test, a nauczyciel widzi wynik i może ocenić odpowiedź manualną;
- późniejsza edycja testu nie zmienia istniejącego podejścia.

Poza zakresem: SchoolAdmin, organizacje, rozliczenia, Live Classroom i kartkówka realtime.

## Etap 8 — skalowanie treści i operacji

Cel: przejść od demonstracyjnego katalogu do kontrolowanego procesu redakcyjnego.

Zakres zależny od danych z produkcji:

- pipeline importu, walidacji i moderacji słownictwa;
- provenance/licencje źródeł oraz raporty jakości;
- rozszerzenie English Grammar o kolejne poziomy;
- kolejne języki i typy ćwiczeń wybierane na podstawie potrzeb;
- object storage i pipeline audio dla Listening;
- optymalizacja wyszukiwania PostgreSQL; zewnętrzny silnik dopiero po pomiarach;
- narzędzia redakcyjne, diff i rollback rewizji;
- backup/restore drills, obserwowalność i SLO.

10 000 słów dla wielu języków jest programem content/data, a nie pojedynczym zadaniem programistycznym. Wymaga źródeł z właściwymi licencjami, normalizacji, deduplikacji, moderacji i metryk jakości.

## Etapy po MVP

### SaaS i School Plans

- Organization, OrganizationMember i SchoolAdmin;
- ownership materiałów i klas przez organizację;
- Plan, Entitlement, Subscription i integracja billing;
- izolacja tenantów, testy bezpieczeństwa i limity planów;
- dopiero potem School Plans.

### Rozszerzona nauka

- adaptacyjny spaced repetition po analizie danych prób;
- Listening z wersjonowanymi assetami audio;
- Writing z rubric/manual review, opcjonalnie później wspomagane AI;
- Speaking po osobnej analizie prywatności, kosztów i jakości;
- SentenceOrdering, MatchPairs i kolejne typy przez Exercise Engine;
- daily/weekly quests i okresowe rankingi.

### Live Classroom i Whiteboard

- model trwałej sesji lekcji oraz protokół zdarzeń;
- SignalR groups, reconnect, kolejność i deduplikacja komunikatów;
- whiteboard strokes + okresowe snapshoty, nigdy bitmapa po każdym ruchu;
- quizy live, wybór ucznia, punkty i kartkówka oparta o Assessments;
- backplane i skalowanie horyzontalne dopiero po testach obciążeniowych;
- widok 2D jako osobny klient/feature korzystający z tych samych kontraktów sesji.

## Elementy, których nie należy implementować teraz

- mikroserwisy lub osobne bazy per moduł;
- platforma pluginów i dynamicznie ładowany kod ćwiczeń;
- uniwersalny workflow engine;
- pełny CMS i pełny zestaw typów ćwiczeń;
- rozbudowana gamifikacja przed stabilnym logiem aktywności;
- SignalR „na zapas”;
- offline-first z dwukierunkową synchronizacją;
- organizacje i billing bez zweryfikowanego Teacher Mode;
- masowy import słownictwa bez provenance, licencji i procesu jakości;
- optymalizacje oraz dodatkowe usługi infrastrukturalne bez pomiarów.

## Wspólna Definition of Done każdego etapu

- przypadki użycia mają jawne kryteria akceptacji;
- logika domenowa ma unit testy, a krytyczne endpointy integration testy;
- autoryzacja zawiera testy pozytywne i negatywne;
- migracja działa od pustej bazy i na kopii stanu z poprzedniego etapu;
- API używa DTO, walidacji i spójnego `ProblemDetails`;
- logi nie zawierają sekretów, kluczy odpowiedzi ani prywatnej treści;
- interfejs jest sprawdzony mobile-first, klawiaturą i podstawowym czytnikiem ekranu;
- dokumentacja i ADR są aktualizowane wraz z decyzją, nie po fakcie;
- funkcja ma podstawowe telemetryczne potwierdzenie działania;
- nie rozpoczynamy kolejnego modułu, dopóki poprzedni pionowy przepływ nie jest używalny.

## Następne zadanie implementacyjne

Kolejne osobne zadanie powinno dotyczyć wyłącznie **Etapu 2 — Vocabulary: pierwszy pionowy slice**. Najpierw należy doprecyzować inwarianty `Concept`, `LexemeSense` i `Lexeme`, provenance oraz widoczność prywatnych danych. Nie rozpoczynamy jeszcze Exercise Engine ani Content Engine.
