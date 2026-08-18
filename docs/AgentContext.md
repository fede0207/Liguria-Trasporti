# Agent context

Questo file serve a riprendere il lavoro senza perdere contesto dopo chiusura IDE o nuova sessione.

## Progetto

- Nome: Liguria Trasporti API
- Stack: ASP.NET Core Web API, .NET 10, EF Core, SQL Server, Scalar/OpenAPI
- Workspace: `D:\CSharp\Liguria Trasporti`
- Approccio API attuale: controller MVC con `[ApiController]`, non Minimal APIs

## Database

- Container dedicato: `liguria-trasporti-sqlserver` (gestito da `docker-compose.yml` nella root del progetto)
- Porta esposta: `localhost:1433`
- Database: `LiguriaTrasportiDb`
- User secret corretto:

  ```text
  ConnectionStrings:DefaultConnection = Server=localhost,1433;Database=LiguriaTrasportiDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True
  ```

## Stato database

Tutte le migration sono applicate a `LiguriaTrasportiDb`.

Tabelle presenti:

- `__EFMigrationsHistory`
- `Employees` (con `FirebaseId`)
- `Shipments`

Per avviare il container:

```bash
docker compose up -d
```

## Decisioni tecniche prese

- Usare DTO separati per request/response.
- Non esporre entity EF Core direttamente dagli endpoint API.
- Usare `[ApiController]` e DataAnnotations per validazioni semplici.
- Non duplicare nei controller controlli gia' gestiti dal framework, per esempio null/empty/whitespace su campi `[Required]`.
- ASP.NET Core valida stringhe whitespace con `[Required]`, ma non fa `Trim()` automatico.
- La normalizzazione input va fatta esplicitamente nel service o nel mapping DTO -> entity.
- Per email conviene salvare `Trim().ToLowerInvariant()`.
- Regole di business e controlli su database devono stare nei servizi applicativi o nel dominio, non nei controller.
- Autorizzazione per ruolo solo nel controller tramite `[Authorize(Roles = "...")]` — il service non riceve `userRole`.
- `ClaimTypes.Role` per leggere il ruolo nei controller, non il claim grezzo Firebase `"role"`.
- Per UID Firebase nel token usare il claim `sub` (canonico JWT/Firebase) in `OnTokenValidated`.
- In `OnTokenValidated` applicare gate applicativo: `FirebaseId` presente, employee esistente, `AccountStatus != Disabled`; in caso contrario `context.Fail(...)`.
- Services e DTOs organizzati in sottocartelle per feature (`Customers`, `Employers`, `Shipments`).

## Struttura cartelle

```
Controllers/
DTOs/
  Customer/
  (Employee e Shipment ancora nella root DTOs)
Services/
  Customers/
  Employers/
  Shipments/
Models/
Enums/
Data/
Migrations/
Authentication/
```

## Stato Employee API

File principali:

- `Controllers/EmployeeController.cs`
- `Services/Employers/IEmployeeService.cs`
- `Services/Employers/EmployeeService.cs`
- `DTOs/EmployeeRequestDto.cs`
- `DTOs/EmployeeResponseDto.cs`
- `Models/Employee.cs`

Stato attuale (completo salvo fix pendenti in NextFixes.md):

- `GET all` — ritorna lista di `EmployeeResponseDto`
- `GET by id` — route nominata `GetEmployeeById`, `404` se non trovato
- `POST create` — `201 Created` con `CreatedAtRoute`, `409 Conflict` su email duplicata, `400` su validazione — **temporaneamente `[AllowAnonymous]` per seed admin**
- `PUT update` — `204 NoContent`, `404`, `409 Conflict`, `400` su validazione
- `DELETE remove` — `204 NoContent`, `404` se non trovato
- trim di `Name`, `Surname`, `Email` nel create/update
- normalizzazione email con `ToLowerInvariant()`
- patente obbligatoria nel create/update quando `Role` e' `Driver`
- patente azzerata a `null` nel create/update quando `Role` non e' `Driver`
- `ServiceResult<T>` su tutta l'Employee API
- controllo email duplicata con `409 Conflict`
- autorizzazione `[Authorize(Roles = "EmployeeManager")]` sull'intero controller
- Firebase: crea utente, assegna custom claim `role`, rollback se DB fallisce
- `FirebaseId` salvato come `string` (uid Firebase)
- Middleware auth: mappa claim Firebase `"role"` in `ClaimTypes.Role` solo se non gia' presente

## Stato Shipment API

File principali:

- `Controllers/ShipmentController.cs`
- `Services/IShipmentService.cs`
- `Services/ShipmentService.cs`
- `DTOs/ShipmentRequestDto.cs`
- `DTOs/ShipmentResponseDto.cs`
- `DTOs/ValidateShipmentRequestDto.cs`
- `Models/Shipment.cs`

Stato attuale (completo):

- `GET all` — ritorna lista di `ShipmentResponseDto`
- `GET by id` — `404` se non trovato
- `POST create` — `201 Created`, accessibile a `LogisticOperator` e `ShippingManager`
- `PATCH {id}/status` — valida spedizione, assegna autista e mezzo, passa a `Planned`; solo `ShippingManager`
- `ServiceResult<T>` su tutta la Shipment API
- autorizzazione solo nel controller tramite `[Authorize(Roles = "...")]`
- controllo stato spedizione (`InPlanning`) prima di validare
- controllo autista: deve esistere e avere ruolo `Driver`
- controllo veicolo: in attesa della tabella `Vehicles`

Problemi/cleanup da fare sono documentati in `docs/NextFixes.md`.

## Promemoria prossima sessione

Prima di modificare codice:

1. Leggere `docs/NextFixes.md`.
2. Controllare `git status --short`.
3. Rileggere i file Employee se il task riguarda dipendenti.
4. Verificare che la connection string punti a `LiguriaTrasportiDb`.
5. Non eseguire migration su `GameDevDb`.

Comandi utili:

```bash
dotnet user-secrets list
dotnet build
dotnet ef database update
```

Per controllare i database nel container:

```bash
docker ps
docker exec GameDev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_strong_password123!" -C -Q "SELECT name FROM sys.databases ORDER BY name"
```

## Warning noti

- `dotnet build` passa.
- Rimane warning `NU1903` su `Microsoft.OpenApi 2.0.0` con vulnerabilita' alta.
- `dotnet-ef` risulta `10.0.7`, runtime EF `10.0.10`.
