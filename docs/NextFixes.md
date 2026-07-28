# Cose da sistemare prossima volta

## Employee API

Stato attuale:

- `GET api/Employee` restituisce `EmployeeResponseDto`.
- `GET api/Employee/{id}` ha route dedicata e nome `GetEmployeeById`.
- `POST api/Employee` restituisce `201 Created` con `CreatedAtRoute`.
- `PUT api/Employee/{id}` aggiorna un dipendente esistente e restituisce `204 NoContent`.
- `DELETE api/Employee/{id}` elimina un dipendente esistente e restituisce `204 NoContent`.
- `Name`, `Surname` ed `Email` vengono normalizzati nel service.
- `Email` viene salvata lowercase con `ToLowerInvariant()`.
- Nel create, se `Role` e' `Driver`, `DrivingLicenseCategory` e' obbligatoria.
- Nell'update, se `Role` e' `Driver`, `DrivingLicenseCategory` e' obbligatoria.

Prossimi fix:

1. Chiarire il comportamento della patente quando il ruolo non e' `Driver`.

   Oggi l'update valorizza `DrivingLicenseCategory` solo quando il ruolo e'
   `Driver`. Se un dipendente passa da `Driver` a `LogisticsOperator` o
   `ShippingManager`, bisogna decidere se azzerare esplicitamente la patente:

   ```csharp
   employee.DrivingLicenseCategory = null;
   ```

   Questa scelta evita di conservare dati non piu' applicabili al ruolo.

2. Sostituire `null` e `bool` con un risultato applicativo esplicito.

   Le firme attuali comunicano poco:

   ```csharp
   Task<EmployeeResponseDto?> CreateEmployee(EmployeeRequestDto employeeRequest);
   Task<bool> UpdateEmployee(Guid id, EmployeeRequestDto employeeRequest);
   Task<bool> DeleteEmployee(Guid id);
   ```

   Meglio introdurre un risultato applicativo che distingua almeno:

   - `Success`
   - `NotFound`
   - `ValidationError`
   - `Conflict`

   In questo modo il service comunica l'esito reale e il controller traduce verso
   `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound` o `409 Conflict`.

3. Aggiungere controllo email duplicata.

   Prima di creare o aggiornare un employee, verificare che non esista gia' un altro
   dipendente con la stessa email normalizzata.

   Risposta consigliata:

   - `409 Conflict` se la richiesta e' formalmente valida ma l'email e' gia' usata.

4. Convertire gli enum come stringhe nell'API.

   In `Program.cs`, configurare `JsonStringEnumConverter` per evitare payload con
   valori numerici poco leggibili.

   ```csharp
   builder.Services
       .AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
       });
   ```

   Aggiungere anche:

   ```csharp
   using System.Text.Json.Serialization;
   ```

   Per ora va bene convertire gli enum come stringhe solo nell'API JSON. La
   persistenza EF Core puo' continuare a usare il mapping attuale.

5. Introdurre log strutturati.

   Possibile approccio:

   - usare Serilog per request logging HTTP globale;
   - loggare nel service eventi applicativi come creazione, update, delete,
     employee non trovato, email duplicata, driver senza patente;
   - non usare il log come meccanismo di controllo del flusso;
   - lasciare al risultato applicativo il compito di dire al controller quale
     risposta HTTP restituire.

## Validazione

1. Mantenere nei DTO solo validazioni semplici.

   Usare `DataAnnotations` per obbligatorieta', lunghezze e formato email.

2. Tenere nei servizi le regole di business.

   Esempi:

   - email gia' esistente;
   - patente obbligatoria se il dipendente e' autista;
   - stati iniziali assegnati dal backend;
   - transizioni valide delle spedizioni.

3. Restituire errori coerenti.

   - `400 BadRequest` per input formalmente non valido o regole di validazione.
   - `404 NotFound` per risorse inesistenti.
   - `409 Conflict` per duplicati o conflitti con lo stato corrente.
