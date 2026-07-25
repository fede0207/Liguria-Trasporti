# Dev notes

Queste note raccolgono le regole operative per sviluppare la Web API ASP.NET Core del progetto.
L'ordine segue il flusso normale di sviluppo: setup, dominio, persistenza, servizi, API, validazione, documentazione, test e problemi comuni.

## 1. Setup iniziale

### Pacchetti principali

- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Design
- Scalar.AspNetCore

### Configurazione locale

1. Inizializzare gli user secrets:
   ```bash
   dotnet user-secrets init
   ```

2. Salvare la connection string fuori dal repository:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=LiguriaTrasportiDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
   ```

Non inserire password reali, token o connection string private in `appsettings.json`.

## 2. Dominio e modello dati

### Employee module checkpoint

1. `Employee` rappresenta utenti/lavoratori aziendali nell'MVP.
2. Campi principali:
   - `Id`
   - `Name`
   - `Surname`
   - `Email`
   - `Role`
   - `OperationalStatus`
   - `AccountStatus`
   - `DrivingLicenseCategory`, nullable perche' applicabile solo agli autisti

3. Enum principali:
   - `EmployeeRole`: `LogisticsOperator`, `ShippingManager`, `Driver`
   - `EmployeeOperationalStatus`: `Active`, `Absent`, `Unavailable`
   - `AccountStatus`: `Active`, `Disabled`
   - `DrivingLicenseCategory`: categorie guida semplificate per i controlli MVP

### Separazione DTO/entity

- Le entity EF Core modellano lo stato persistito e le relazioni.
- I request DTO modellano l'input ricevuto dal client.
- I response DTO modellano l'output pubblico dell'API.
- Non esporre direttamente come input campi controllati dal backend, per esempio `Id`, stati calcolati, audit fields o timestamp di sistema.
- Creare DTO separati per create, update e response quando la forma dei dati e' diversa.

## 3. Persistenza con EF Core

### Creare il DbContext

1. Creare una classe che eredita da `DbContext`.
2. Ricevere `DbContextOptions<AppDbContext>` nel costruttore.
3. Esporre un `DbSet<TEntity>` per ogni aggregate/entity persistita.

Esempio:

```csharp
public DbSet<Employee> Employees => Set<Employee>();
```

### Registrare EF Core

In `Program.cs`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### Migration

Creare una migration:

```bash
dotnet ef migrations add InitialCreate
```

Applicare le migration:

```bash
dotnet ef database update
```

## 4. Servizi applicativi

### Creare un servizio

1. Definire un contratto, per esempio `IShipmentService`.
2. Inserire nel contratto solo le operazioni necessarie ai controller.
3. Implementare il servizio in una classe concreta, per esempio `ShipmentService`.
4. Iniettare `AppDbContext` nel costruttore del servizio.
5. Usare metodi async di EF Core, per esempio `ToListAsync`, `FindAsync`, `SingleOrDefaultAsync`.

### Registrare i servizi

In `Program.cs`:

```csharp
builder.Services.AddScoped<IShipmentService, ShipmentService>();
```

La classe concreta deve implementare l'interfaccia registrata.

## 5. Controller e API

### Registrazione controller

In `Program.cs`:

```csharp
builder.Services.AddControllers();
```

E dopo la costruzione dell'app:

```csharp
app.MapControllers();
```

`MapControllers()` deve stare fuori dal blocco Development-only, altrimenti gli endpoint non sono disponibili in produzione.

### Struttura controller

1. Usare `[ApiController]`.
2. Usare route esplicite, per esempio `[Route("api/[controller]")]`.
3. Iniettare i servizi tramite costruttore.
4. Usare attributi HTTP chiari: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`.
5. Non mettere logica di business complessa nel controller.

Esempio:

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        this.employeeService = employeeService;
    }
}
```

## 6. Validazione backend

La validazione backend e' obbligatoria anche se esiste una validazione frontend.
Il frontend migliora l'esperienza utente, ma non e' una barriera di sicurezza.

### Cosa gestisce ASP.NET Core automaticamente

Con controller annotati con `[ApiController]`, ASP.NET Core:

- Esegue il model binding da body, route, query string, header e form.
- Registra in `ModelState` gli errori di conversione, per esempio una stringa non numerica ricevuta per un `int`.
- Applica gli attributi `System.ComponentModel.DataAnnotations` sui DTO.
- Restituisce automaticamente `400 Bad Request` quando `ModelState` non e' valido.
- Usa `ValidationProblemDetails` come formato standard per gli errori di validazione.

Attributi utili:

- `[Required]`
- `[StringLength]`
- `[MaxLength]`
- `[MinLength]`
- `[Range]`
- `[EmailAddress]`
- `[Phone]`
- `[RegularExpression]`

Esempio:

```csharp
public sealed class CreateEmployeeRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
}
```

Con nullable reference types abilitate, una proprieta' non nullable comunica che il valore non dovrebbe essere `null`.
Per chiarezza delle API pubbliche, usare comunque attributi espliciti sui request DTO quando il campo e' obbligatorio.

### Cosa deve implementare l'applicazione

Implementare nel codice applicativo, nei servizi o in validatori dedicati:

- Regole di business, per esempio "una spedizione puo' passare a `InConsegna` solo se e' gia' pianificata".
- Regole cross-field, per esempio "la data consegna deve essere successiva alla data di creazione".
- Controlli su database, per esempio email dipendente gia' presente o mezzo non disponibile.
- Controlli legati all'utente corrente, per esempio ruolo, ownership o permessi sulla risorsa.
- Invarianti di dominio che devono valere anche fuori dal layer HTTP.
- Normalizzazione input, per esempio trim delle stringhe e normalizzazione email.
- Mapping coerente degli errori applicativi verso risposte HTTP.

Per regole cross-field semplici e locali al DTO si puo' usare `IValidatableObject`:

```csharp
public sealed class PlanShipmentRequest : IValidatableObject
{
    public DateOnly ExpectedDeliveryDate { get; set; }
    public int DriverId { get; set; }
    public int VehicleId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpectedDeliveryDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult(
                "Expected delivery date cannot be in the past.",
                [nameof(ExpectedDeliveryDate)]);
        }
    }
}
```

Per regole che richiedono accesso a database, servizi esterni o utente corrente, preferire un servizio applicativo o un validatore dedicato invece di `DataAnnotations`.

### Regole pratiche per questo progetto

- Validare i request DTO al bordo dell'API.
- Non validare direttamente le entity EF come se fossero input pubblico.
- Usare `DataAnnotations` solo per vincoli semplici di formato, lunghezza, range e obbligatorieta'.
- Tenere le regole del ciclo spedizione nei servizi applicativi o nel dominio.
- Restituire errori di validazione con `ValidationProblemDetails`.
- Restituire conflitti di stato o duplicati con un errore applicativo coerente, tipicamente `409 Conflict` quando la richiesta e' formalmente valida ma incompatibile con lo stato corrente.
- Coprire con test almeno input mancante, formato errato, limiti minimi/massimi e transizioni di stato non permesse.

### Minimal APIs

Se in futuro il progetto usa Minimal APIs, la validazione integrata va registrata esplicitamente:

```csharp
builder.Services.AddValidation();
```

Per l'attuale approccio a controller, continuare a usare `[ApiController]`, DTO dedicati e `DataAnnotations` dove appropriate.

### Documentazione utile

- ASP.NET Core validation overview: https://learn.microsoft.com/en-us/aspnet/core/validation/overview
- Model validation in ASP.NET Core MVC: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
- Create web APIs with ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- Minimal APIs validation: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- `ValidationProblemDetails`: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails
- `System.ComponentModel.DataAnnotations`: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations

## 7. OpenAPI e Scalar

### Registrazione OpenAPI

In `Program.cs`:

```csharp
builder.Services.AddOpenApi();
```

### Esposizione documentazione

Mappare la documentazione solo in sviluppo:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

## 8. Error handling

Regole da seguire quando verranno introdotte risposte applicative strutturate:

- Usare `ValidationProblem(...)` per errori di validazione custom, cosi' il formato resta coerente con gli errori automatici.
- Usare `NotFound(...)` quando la risorsa richiesta non esiste.
- Usare `Conflict(...)` per conflitti con lo stato corrente, per esempio assegnazione a mezzo non disponibile.
- Non restituire stack trace o dettagli interni al client.
- Loggare lato server il dettaglio tecnico necessario al debug.

## 9. Test

Test minimi da aggiungere man mano che i moduli crescono:

- Unit test dei servizi applicativi per regole di business e transizioni di stato.
- Integration test degli endpoint per model binding, validazione DTO e codici HTTP.
- Test EF Core solo quando serve verificare query, vincoli o mapping significativi.

## 10. Problemi comuni

1. Riga `using` vuota:
   - una riga come `using` senza namespace rompe il parsing di `Program.cs`.

2. Registrazione servizio errata:
   - `AddScoped<IService, Service>()` richiede che `Service` implementi `IService`.

3. Controller registrati ma non mappati:
   - `AddControllers()` registra i servizi MVC/controller;
   - `MapControllers()` espone gli endpoint con attribute routing.

4. Chiamata errata ai metodi dei servizi:
   - chiamare i metodi tramite il servizio iniettato;
   - non chiamare un metodo del servizio come se fosse un metodo locale del controller.

5. Build bloccata dall'app in esecuzione:
   - se l'eseguibile e' bloccato, fermare l'app da Rider oppure terminare il processo tramite id.

6. Confusione tra account status e operational status:
   - `AccountStatus` decide se l'utente puo' accedere al sistema;
   - `OperationalStatus` decide se il dipendente e' disponibile per lavoro/pianificazione.
