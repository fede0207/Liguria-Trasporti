# Agent context

Questo file serve a riprendere il lavoro senza perdere contesto dopo chiusura IDE o nuova sessione.

## Progetto

- Nome: Liguria Trasporti API
- Stack: ASP.NET Core Web API, .NET 10, EF Core, SQL Server, Scalar/OpenAPI
- Workspace: `D:\CSharp\Liguria Trasporti`
- Approccio API attuale: controller MVC con `[ApiController]`, non Minimal APIs

## Database

- Container SQL Server attivo usato localmente: `GameDev`
- Porta esposta: `localhost,1433`
- Database corretto per questo progetto: `LiguriaTrasportiDb`
- Database da non usare per questo progetto: `GameDevDb`
- User secret corretto:

  ```text
  ConnectionStrings:DefaultConnection = Server=localhost,1433;Database=LiguriaTrasportiDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True
  ```

## Stato database

Le migration del progetto Liguria sono state applicate a `LiguriaTrasportiDb`.

Tabelle attese in `LiguriaTrasportiDb`:

- `__EFMigrationsHistory`
- `Employees`
- `Shipments`

Migration attese:

- `20260723125822_InitiaCreate`
- `20260723150025_CreateEmployees`

Nota importante: in una sessione precedente le migration erano state applicate per errore a `GameDevDb`.
La pulizia e' gia' stata fatta rimuovendo solo:

- `dbo.Employees`
- `dbo.Shipments`
- righe Liguria in `dbo.__EFMigrationsHistory`

Non toccare `GameDevDb`: appartiene a un altro progetto.

## Decisioni tecniche prese

- Usare DTO separati per request/response.
- Non esporre entity EF Core direttamente dagli endpoint API.
- Usare `[ApiController]` e DataAnnotations per validazioni semplici.
- Non duplicare nei controller controlli gia' gestiti dal framework, per esempio null/empty/whitespace su campi `[Required]`.
- ASP.NET Core valida stringhe whitespace con `[Required]`, ma non fa `Trim()` automatico.
- La normalizzazione input va fatta esplicitamente nel service o nel mapping DTO -> entity.
- Per email conviene salvare `Trim().ToLowerInvariant()`.
- Regole di business e controlli su database devono stare nei servizi applicativi o nel dominio, non nei controller.

## Stato Employee API

File principali:

- `Controllers/EmployeeController.cs`
- `Services/IEmployeeService.cs`
- `Services/EmployeeService.cs`
- `DTOs/EmployeeRequestDto.cs`
- `DTOs/EmployeeResponseDto.cs`
- `Models/Employee.cs`

Stato attuale:

- `GET all`
- `GET by id`
- `POST create`
- `PUT update`
- `DELETE remove`
- `CreatedAtRoute` nel create verso la route nominata `GetEmployeeById`
- `204 NoContent` per update/delete riusciti
- trim di `Name`, `Surname`, `Email` nel create/update
- normalizzazione email con `ToLowerInvariant()`
- patente obbligatoria nel create quando `Role` e' `Driver`
- patente obbligatoria nell'update quando `Role` e' `Driver`
- protezione `[Authorize]` su `GET by id`, `PUT` e `DELETE`
- `[ApiController]` e route base sul controller

Problemi/cleanup da fare sono documentati in `docs/NextFixes.md`.
In particolare restano:

- decidere se azzerare `DrivingLicenseCategory` quando il ruolo aggiornato non e' `Driver`;
- sostituire `null`/`bool` dai service con un risultato applicativo esplicito;
- aggiungere controllo email duplicata;
- convertire gli enum come stringhe nell'API JSON;
- introdurre log strutturati, probabilmente con Serilog.

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
