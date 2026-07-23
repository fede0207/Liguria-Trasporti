# Liguria Trasporti API

Web API sviluppata in ASP.NET Core per gestire le attività operative di una piccola azienda di trasporti.

Il progetto nasce come applicazione da portfolio backend e simula un caso aziendale reale, concentrandosi sulla progettazione di API REST, gestione dei dati relazionali, autenticazione, autorizzazione e tracciamento delle operazioni.

## Obiettivo

L'applicazione permette di gestire il ciclo completo di una spedizione, dalla registrazione del cliente fino alla consegna o all'eventuale annullamento.

Gli utenti dell'applicazione appartengono a diversi ruoli aziendali:

- Operatore logistico
- Responsabile spedizioni
- Autista

Ogni ruolo può eseguire solo le operazioni per cui è autorizzato.

## Funzionalità previste

- Gestione dei clienti
- Gestione degli indirizzi
- Creazione e modifica delle spedizioni
- Proposta di un autista e di un mezzo
- Conferma dell'assegnazione da parte del responsabile
- Gestione dello stato della spedizione
- Gestione dei tentativi di consegna
- Registrazione dei problemi durante una consegna
- Visualizzazione delle consegne previste per data
- Visualizzazione del giro assegnato a un autista
- Gestione degli autisti e delle loro abilitazioni
- Gestione dei mezzi aziendali
- Controllo della disponibilità di autisti e mezzi
- Audit log delle operazioni effettuate
- Autenticazione tramite provider esterno
- Autorizzazione basata su ruoli e policy

## Ciclo di una spedizione

Gli stati principali previsti sono:

```text
Da pianificare
    ↓
Pianificata
    ↓
In consegna
    ↓
Consegnata
```

Stati alternativi:

```text
Consegna fallita
Annullata
```

Una spedizione viene considerata pianificata solo dopo che il responsabile ha confermato l'autista e il mezzo proposti dall'operatore logistico.

## Tecnologie

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- Provider di autenticazione esterno
- Serilog
- OpenAPI
- Scalar
- Docker

## Struttura principale del dominio

Le principali entità previste sono:

- Cliente
- Indirizzo
- Spedizione
- Dipendente
- Autista
- Mezzo
- GiroAutista
- TentativoConsegna
- Segnalazione
- AuditLog

## Relazioni principali

- Un cliente può avere molti indirizzi
- Un cliente può avere molte spedizioni
- Una spedizione ha un indirizzo di partenza
- Una spedizione ha un indirizzo di destinazione
- Una spedizione può essere assegnata a un autista
- Una spedizione può essere assegnata a un mezzo
- Una spedizione può avere molti tentativi di consegna
- Un giro autista contiene molte spedizioni
- Un giro autista è assegnato a un autista
- Un giro autista utilizza un mezzo

## Requisiti

Per eseguire il progetto sono necessari:

- .NET 10 SDK
- SQL Server
- Docker Desktop, opzionale
- JetBrains Rider, Visual Studio o Visual Studio Code

## Configurazione

Clonare il repository:

```bash
git clone <repository-url>
cd <repository-name>
```

Ripristinare le dipendenze:

```bash
dotnet restore
```

Configurare la stringa di connessione tramite User Secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=LiguriaTrasportiDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

Non inserire password reali o credenziali nel repository.

## Database

Creare una migration:

```bash
dotnet ef migrations add InitialCreate
```

Applicare le migration:

```bash
dotnet ef database update
```

## Avvio

Avviare l'applicazione:

```bash
dotnet run
```

La documentazione OpenAPI sarà disponibile attraverso Scalar nell'ambiente di sviluppo.

## Stato del progetto

Il progetto è attualmente in sviluppo.

Le funzionalità, il modello dati e le regole di business verranno implementati progressivamente.

## Obiettivi formativi

Il progetto viene utilizzato per approfondire:

- Progettazione di Web API REST
- Modellazione di database relazionali
- Entity Framework Core
- Dependency Injection
- Separazione delle responsabilità
- Validazione degli input
- Gestione degli errori
- Logging strutturato
- Audit logging
- Autenticazione e autorizzazione
- Testing automatico
- Docker e configurazione degli ambienti
