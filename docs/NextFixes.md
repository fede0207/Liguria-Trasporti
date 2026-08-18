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
- `FirebaseId` salvato come `string` nel DB e nel modello.

## Customer API

Stato attuale:

- `GET all` e `GET by id` implementati.
- `POST create` implementato con controllo email e phone duplicati.
- `ICustomerService` non ha ancora tutti i metodi (`CreateCustomer` manca dall'interfaccia).
- `ICustomerService` non registrato in `Program.cs`.

Prossimi fix:

1. Aggiungere `CreateCustomer` all'interfaccia `ICustomerService`.
2. Registrare `ICustomerService` in `Program.cs`.
3. Normalizzare `Email` e `PhoneNumber` prima dei controlli duplicati e del salvataggio.
4. Rimuovere `using Microsoft.AspNetCore.Http.HttpResults` non usato in `CustomerService`.
5. Creare `CustomerController`.
6. Aggiungere metodi `UpdateCustomer` e `DeleteCustomer`.

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
