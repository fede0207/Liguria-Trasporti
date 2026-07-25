# Cose da sistemare prossima volta

## Employee API

1. Sistemare la route del GET by id.

   Ora ci sono due endpoint `[HttpGet]` sullo stesso controller.
   Il metodo by id deve avere una route dedicata:

   ```csharp
   [HttpGet("{id:guid}")]
   public async Task<ActionResult<EmployeeResponseDto?>> GetEmployeeById(Guid id)
   ```

2. Non esporre entity EF Core dagli endpoint.

   `GetAllEmployees` attualmente ritorna `Employee`.
   Meglio restituire `EmployeeResponseDto`, come gia' fatto nel POST.

   Da cambiare nell'interfaccia:

   ```csharp
   Task<IEnumerable<EmployeeResponseDto>> GetAllEmployees();
   Task<EmployeeResponseDto?> GetEmployeeById(Guid id);
   ```

3. Valorizzare `Id` nella response del POST.

   `EmployeeResponseDto` ha la proprieta' `Id`, ma nel mapping del create va assegnata:

   ```csharp
   Id = employee.Id,
   ```

4. Rendere non nullable il risultato del create.

   `CreateEmployee` crea sempre un dipendente oppure fallisce.
   La firma puo' essere:

   ```csharp
   Task<EmployeeResponseDto> CreateEmployee(EmployeeRequestDto employeeRequest);
   ```

5. Restituire `201 Created` nel POST.

   Nel controller, dopo la creazione, usare `CreatedAtAction` invece di `Ok`:

   ```csharp
   var employee = await _employeeService.CreateEmployee(employeeRequest);

   return CreatedAtAction(
       nameof(GetEmployeeById),
       new { id = employee.Id },
       employee);
   ```

6. Normalizzare email.

   Il trim su nome, cognome ed email va bene.
   Per email conviene aggiungere anche lowercase:

   ```csharp
   Email = employeeRequest.Email.Trim().ToLowerInvariant(),
   ```

   Questo evita duplicati logici quando verra' aggiunto il controllo di unicita'.

## Validazione

1. Aggiungere `[Required]` ai campi obbligatori del request DTO.

   Per esempio:

   ```csharp
   [Required]
   [MaxLength(50)]
   public string Name { get; set; } = string.Empty;
   ```

2. Lasciare al framework i controlli base.

   Con `[ApiController]`, ASP.NET Core restituisce automaticamente `400 Bad Request` quando il DTO non e' valido.
   Non serve duplicare nel controller controlli tipo `string.IsNullOrWhiteSpace(...)`.

3. Tenere nei servizi le regole di business.

   Per esempio:
   - email gia' esistente;
   - patente obbligatoria se il dipendente e' autista;
   - stati iniziali assegnati dal backend;
   - transizioni valide delle spedizioni.
