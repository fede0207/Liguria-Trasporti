# Consegna:
Azienda di trasporti Liguria Trasporti Srl, con 25 autisti e 18 mezzi chiede
applicazione backend ASP.NET Core Web API che permette di:

- Registrare Clienti e Indirizzi di consegna
- Inserire una nuova Spedizione
- Assegnare spedizione ad autista e mezzo
- Aggiornare Stato Consegna
- Registrare eventuali problemi
- Visualizzare le consegne previste in una determinata giornata

Stato Spedizione: Da pianificare -> Pianificata -> In Consegna -> Consegnata
Consegna Fallita
Annullata

# Spedizione: 
- Codice identificativo
- Cliente
- Indirizzo di partenza
- Indirizzo di arrivo
- Data di consegna prevista
- Stato
- Autista
- Mezzo
- Eventuali note

La spedizione puo' essere creata da un operatore logistico, ma deve essere assegnata dal responsabile delle spedizioni.
Il responsabile sceglie:
- Data prevista di consegna
- Autista
- Mezzo
- Eventuale ordine delle consegne della giornata

Non tutte le spedizioni vengono assegnate subito, alcune possono rimanere "Da Pianificare" per piu'giorni.
Una assegnazione puo' essere modificata prima della partenza, per esempio se:
- Autista assente
- Mezzo guasto
- Arriva consegna piu'urgente
- Il carico supera la capacita' del mezzo

Dopo che l'autista ha iniziato il giro, l'ufficio puo'cambiare l'assegnazione, ma solo in casi eccezionali.

Una volta che la spedizione é "Pianificata", il responsabile la assegna al giro dell'autista. Quando l'autista parte, a quel punto la spedizione passa "In Consegna". 
Caso ordinario: Autista completa consegna, aggiorna spedizione a "Consegnata"
a quel punto, devono essere registrati:
- data e ora consegna
- nota

Caso non ordinario: autista deve indicare il motivo della consegna fallita.
I casi piu' comuni sono:
- destinatario assente
- Indirizzo errato o incompleto
- merce rifiutata
- accesso impossibile
- problema al mezzo
- merce danneggiata
- altro motivo da speicifare

In questo caso la spedizione passa a "Consegna Fallita", ma non sempre viene considerata conclusa.
Il responsabile valuta cosa fare: 
- Riprogrammare tentativo
- assegnare un altro autista
- assegnare un altro mezzo
- correggere i dati spedizione
- annullare definitivamente la consegna

Quando viene riprogrammato un nuovo tentativo, vogliamo conservare lo storico del tentativo precedente e 
del relativo motivo di fallimento
Eccezioni operative:
- Caso guasto: autista segnala che non puo' proseguire ->
Spedizioni rimangono aperte -> responsabile decide se riassegnarle o riprogrammarle

- Caso destinatario chiede modifica urgente:
autista puo' segnalare la richiesta -> ufficio/responsabile approva e apporta la modifica

- Caso Spedizione annullata prima della partenza: 
spedizione non entra in stato In Consegna.
Deve risultare una motivazione.

- Caso spedizione annullata in consegna:
conservare chi dispone annullamento e motivazioni
data e ora annullamento

Conservare storico passaggi principali:
- creazione 
- assegnazione
- modifiche eccezionali
- partenza
- tentativi di consegna
- completamento/fallimento/annullamento

# Operatore Logistico:
- Creare spedizione
- modificarne i dati fino alla partenza
- proporre un autista e mezzo
- visualizzare le spedizioni

# Responsabile spedizioni:
- Cambiare autista o mezzo
- riprogrammare spedizione
- annullare assegnazione
- intervenire su spedizioni gia' pianificate

# Gestione Autisti
Registrare e gestire:
- Operatori logistici
- Responsabile Spedizioni
- Autisti
- Stato dipendente(Attivo, Assente, Non disponibile)
- eventuale abilitazione alla guida di determinati mezzi

Gli autisti devono accedere il sistema per:
- Vedere spedizioni assegnate
- Indicare quando inizia il giro
- aggiornare lo stato della consegna
- segnalare un problema
- indicare temporaneamente di non poter proseguire, esempio guasto o malessere

La disponibilita' ordinaria dell'autista viene gestita dal responsabile spedizioni, sulla base di turni e comunicazioni interne.

