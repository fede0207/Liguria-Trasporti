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

### ServiceResult pattern

I servizi ritornano `ServiceResult<T>` per comunicare **status**, **data** e **error message** in modo strutturato.

#### Struttura di ServiceResult

```csharp
public sealed class ServiceResult<T>
{
    public bool Success { get; init; }
    public ServiceResultStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }
    
    public static ServiceResult<T> Ok(T data) { ... }
    public static ServiceResult<T> NotFound(T data) { ... }
    public static ServiceResult<T> ValidationError(T data) { ... }
    public static ServiceResult<T> Conflict(T data) { ... }
    public static ServiceResult<T> Unauthorized(T data) { ... }
    public static ServiceResult<T> Forbidden(T data) { ... }
}
```

#### Uso nei servizi

Nel servizio, usa i metodi statici per ritornare risultati:

```csharp
// Successo
return ServiceResult<EmployeeResponseDto>.Ok(employeeData);

// Non trovato
return ServiceResult<EmployeeResponseDto?>.NotFound(null);

// Conflitto (es. email duplicata)
return ServiceResult<EmployeeResponseDto>.Conflict(null!);

// Errore di validazione
return ServiceResult<EmployeeResponseDto>.ValidationError();
```

#### Gestione nel controller

Nel controller, controlla lo `Status` e mappa verso HTTP:

```csharp
var result = await _employeeService.CreateEmployee(request);

return result.Status switch
{
    ServiceResultStatus.Success => CreatedAtRoute(..., result.Data),
    ServiceResultStatus.ValidationError => BadRequest(),
    ServiceResultStatus.Conflict => Conflict(),
    _ => BadRequest("Unknown error")
};
```

## 5.A Autenticazione e Autorizzazione con Firebase

### Flusso di creazione dipendente con Firebase Auth

Quando crei un nuovo dipendente, devi:

1. **Salvare l'Employee nel database** (EF Core)
2. **Creare l'utente su Firebase** con email e password temporanea
3. **Assegnare il custom claim** `role` con il ruolo aziendale (`EmployeeManager`, `Driver`, ecc.)

Nel servizio:

```csharp
public class EmployeeService(AppDbContext dbContext, FirebaseAuth firebaseAuth) : IEmployeeService
{
    public async Task<ServiceResult<EmployeeResponseDto>> CreateEmployee(EmployeeRequestDto request)
    {
        // 1. Validazioni e salvataggio nel DB
        var employee = new Employee { ... };
        await _dbContext.AddAsync(employee);
        await _dbContext.SaveChangesAsync();
        
        // 2. Creare l'utente Firebase
        var userRecord = await _firebaseAuth.CreateUserAsync(new UserRecordArgs()
        {
            Email = employee.Email,
            Password = "temporary_password_123"  // L'utente la cambierà al primo accesso
        });
        
        // 3. Assegnare il ruolo come custom claim
        var claims = new Dictionary<string, object> { { "role", employee.Role.ToString() } };
        await _firebaseAuth.SetCustomUserClaimsAsync(userRecord.Uid, claims);
        
        return ServiceResult<EmployeeResponseDto>.Ok(employeeResponse);
    }
}
```

### Mapping dei custom claims in Program.cs

Nel `Program.cs`, configura `OnTokenValidated` per leggere il custom claim `role` dal token Firebase e aggiungerlo come Role di ASP.NET Core:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters.ValidIssuer = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters.ValidAudience = projectId;
        
        options.Events.OnTokenValidated = context =>
        {
            // Leggi il custom claim "role" dal token Firebase
            var roleClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == "role");
            if (roleClaim != null)
            {
                // Converti il claim in un Role che ASP.NET Core capisce
                var identity = context.Principal.Identity as ClaimsIdentity;
                identity?.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
            return Task.CompletedTask;
        };
    });
```

### Proteggere gli endpoint per ruolo

Nel controller, usa `[Authorize(Roles = "...")]`:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "EmployeeManager")]
public class EmployeeController : ControllerBase { ... }
```

Solo utenti con il ruolo `EmployeeManager` potranno accedere agli endpoint di questo controller.

### Divisione responsabilità: Ruoli e Logica di Business

**Principio:** Autorizzazione nel controller, logica di business nel service.

#### 1. Creare utente e assegnare ruolo → Service

Quando crei un nuovo dipendente, il servizio deve:
- Salvare l'Employee nel database
- Creare l'utente su Firebase
- Assegnare il ruolo come custom claim

**Non devi** sapere chi sta facendo la richiesta. È una semplice creazione di risorsa.

#### 2. Controllare chi può fare cosa → solo Controller

L'autorizzazione per ruolo va gestita tramite `[Authorize(Roles = "...")]` sul controller o sull'action specifica. Il framework blocca la richiesta **prima** di entrare nel metodo se il ruolo non corrisponde — il service non viene mai chiamato e non ha bisogno di conoscere il ruolo.

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "LogisticOperator,ShippingManager")]  // gate generale
public class ShipmentController : ControllerBase
{
    [HttpPatch("{shipmentId:Guid}/status")]
    [Authorize(Roles = "ShippingManager")]  // gate più restrittivo sull'action
    public async Task<IActionResult> ValidateShipmentStatus(...)
    {
        var result = await _shipmentService.ValidateShipment(shipmentId, request);
        // nessun check ruolo qui: ci ha già pensato [Authorize]
    }
}
```

Il service riceve solo i parametri di business, senza `userRole`:

```csharp
Task<ServiceResult<ShipmentResponseDto>> ValidateShipment(Guid shipmentId, ValidateShipmentRequestDto request);
```

#### 3. Leggere il ruolo nel controller (quando serve passarlo)

Usare `User.FindFirstValue(ClaimTypes.Role)` — non leggere il claim grezzo Firebase `"role"`.

```csharp
var userRole = User.FindFirstValue(ClaimTypes.Role);
```

Il token Firebase contiene `"role"` come chiave. `OnTokenValidated` lo traduce in `ClaimTypes.Role` (la chiave che ASP.NET Core usa internamente). Leggere `"role"` funziona per coincidenza, ma `ClaimTypes.Role` è coerente con il resto del framework.

#### Scelta del tipo generico T

- **Se devi ritornare dati:** `ServiceResult<EmployeeResponseDto>` — Usa il DTO della risposta
- **Se non devi ritornare dati:** `ServiceResult<EmptyResponse>` — Usa una classe vuota per operazioni che ritornano `NoContent()`
- **Esempi:**
  - POST create → `ServiceResult<EmployeeResponseDto>` (serve l'ID per `CreatedAtRoute`)
  - PUT update → `ServiceResult<EmptyResponse>` (ritorna `NoContent()`)
  - DELETE → `ServiceResult<EmptyResponse>` (ritorna `NoContent()`)

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
