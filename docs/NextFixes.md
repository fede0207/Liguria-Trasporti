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
- Nell'update, se `Role` non e' `Driver`, `DrivingLicenseCategory` viene azzerata a `null`.
- `ServiceResult<T>` su tutta l'Employee API.
- Controllo email duplicata con `409 Conflict`.

Prossimi fix:

1. Valutare serializzazione enum (in sospeso).

   Decisione da prendere: mantenere enum come interi (default) o convertire in stringhe
   con `JsonStringEnumConverter`. Pro/contro documentati in DevNote sezione 5.A.
   Chiedere conferma allo sviluppatore di riferimento prima di procedere.

2. Introdurre log strutturati.

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
