# Proposte: meno ascolto passivo tra Intro e Scena 1

**Archivio delle proposte iniziali.** Le cinque modifiche sono montate nella versione da 93 elementi. Per testi, controlli e configurazione correnti vedere `docs/scena1-interattiva.md`. Le istruzioni su indice/medio per rispondere presenti qui sotto sono superate: le risposte usano Sì/No del pollice, sulla tastiera Y/N.


Le cinque modifiche di partecipazione della Scena 1 sono approvate dall’11 novembre. Il piano corrente e gli audio da registrare sono in `docs/scena1-interattiva.md` e `docs/registrazioni-scena1-interattiva.csv`, dalla radice del repository. Questo documento conserva le proposte storiche, comprese le opzioni non incluse come il gesto nell’Intro. Del vento Sbuffo resta la descrizione, senza raccontare di nuovo il furto.

**Riferimenti storici:** gli ID e i minuti nel documento si riferiscono alla versione precedente a quest’ultimo taglio. Le vecchie `s01_maes_24` e `25` e il vecchio `s01_sfx_23` sono stati tolti; le voci successive scalano di due, i suoni di uno. Per gli ID correnti e i punti di riascolto vedere il README della storia. Le durate e l’obiettivo di 10–11 minuti sono stime, da rimisurare dopo aver scelto le interazioni.
I testi segnati come **bozza** sono solo un'idea di tono: riscrivili come vuoi.

## Il problema, in numeri

Misurato sui file audio veri, con le pause del gioco:

| | Durata | Il bambino fa qualcosa? |
|---|---|---|
| Intro (`p00`) | circa 5 min | mai |
| Scena 1 (`s01`) fino al primo pulsante (`s01_int_01`) | circa 13,5 min | mai |
| **Totale prima del primo gesto** | **circa 18,5 min** | |

Dove va il tempo nella Scena 1:

| Minuto | Blocco | Battute | Durata |
|---|---|---|---|
| 0:00 | Arriva il Maestro, gag della bacchetta, si presenta, TAC TAC TOC | `sfx_01` – `maes_10` | 2,3 min |
| 2:20 | «Questo è il nostro problema» (la dimostrazione degli strumenti insieme ora è tolta) | `sfx_11` – `maes_13` | 1,5 min |
| 3:50 | Il ritmo: mani, passi, TAC TAC TOC, il pianoforte che imita | `maes_14` – `maes_21` | 3 min |
| 6:50 | Sbuffo: aneddoto dei fogli, poi racconta di nuovo l'arrivo del vento | `sfx_20` – `maes_25` | 1,7 min |
| 8:35 | Il pianoforte a cui manca la nota, i pezzi sparpagliati | `sfx_24` – `maes_30` | 1,8 min |
| 10:25 | «Ti va di aiutarmi?», «non devi essere un musicista», come funzionerà il viaggio | `sfx_28` – `maes_37` | 2,3 min |
| 12:40 | Il Tocco, poi finalmente si preme (13:35) | `sfx_36` – `int_01` | 1 min |

Due problemi oltre alla lunghezza:
- **Il problema della canzone rubata si spiega tre volte**: nell'Intro (il vento ruba la canzone), all'inizio della Scena 1 (`maes_04` «Manca proprio quella nota!», `maes_11` «Questo è il nostro problema») e di nuovo nel blocco del pianoforte (`sfx_24` – `maes_30`).
- **Il Maestro fa domande a cui il bambino non può rispondere**: «Ti sembrano uguali?» (`maes_16`), «Ti andrebbe di aiutarmi?» (`maes_33`). Sono già momenti di interazione, solo che non lo sono ancora.

## L'obiettivo

- Il primo gesto entro circa 3 minuti dall'inizio della Scena 1, e possibilmente un gesto semplice già nell'Intro.
- Poi qualcosa da fare almeno ogni 1–2 minuti.
- Scena 1 da circa 16 minuti a circa 10–11, con 6 momenti in cui il bambino fa qualcosa invece di 2.

---

## Proposta 1: insegnare il Tocco subito (la più importante)

Il Tocco oggi arriva al minuto 12:40. Spostiamolo **subito dopo che il Maestro si presenta** (dopo `maes_10` «Quello sono io!»). Ha anche senso nella storia: lui ha il suo segnale (TAC TAC TOC) e anche il bambino riceve il suo modo di "farsi sentire".

Nuovo ordine:
1. `maes_10` «Quello sono io! …me lo sto chiedendo anch'io.» Va accorciato e chiuso dopo «Quello sono io!», perché la spiegazione del problema arriva dopo il Tocco.
2. **Nuova battuta (bozza)**: «Io ho la mia bacchetta per farmi sentire. Ma anche tu ti meriti un modo per farti sentire! E per fortuna… ce l'hai già.»
3. `maes_39` solo da «Hai tra le mani un oggetto…» in poi. Si toglie la gag del pianoforte scomodo (`maes_38`, `sfx_37` e l'inizio di `maes_39`), che senza il contesto attuale non serve più.
4. `maes_40` (quattro pulsanti), `sfx_38` (quattro segnali).
5. `maes_41` senza «Tocco ci permetterà di partecipare… quale pulsante usare.», poi `s01_int_01` (premere l'indice), `sfx_39` (conferma).
6. `maes_42`, accorciata a «Oh! Quel suono ci dice che hai premuto il pulsante! Magnifico!» più la frase su pianoforte e strumenti.
7. **Nuova battuta di raccordo (bozza)**: «Bene! Adesso che possiamo parlarci… ti racconto che cosa è successo qui.» Poi si riprende con `sfx_11` e `maes_11`.

**Primo gesto: circa al minuto 3 della Scena 1** (adesso è al 13:35).

Da registrare: 2 battute brevi. Il resto si fa tagliando l'audio esistente (`tools/voice_trim.py`).

---

## Proposta 2: il ritmo si fa, non si ascolta soltanto

Il blocco del ritmo (3 minuti) è il cuore del gioco: è proprio la meccanica che Tommi sta costruendo. Oggi il bambino ascolta solo spiegazioni. Proposta:

**2a. Ripetere TA… TA… TA**
- Dopo `sfx_13` (tre battiti lenti) e `maes_15` (fino a «…sempre lo stesso tempo.»):
- **Nuova battuta (bozza)**: «Adesso prova tu! Premi il pulsante dell'indice tre volte, come ho fatto io: TA… TA… TA.»
- **Interazione nuova**: tre pressioni, valutate con larghezza. Nessun errore punitivo: se non va, il Maestro rifà l'esempio.
- **Risposta (bozza)**: «Bravissimo! Hai appena suonato il tuo primo ritmo!»
- Poi si torna a «Ora ascolta quest'altro.» (la fine di `maes_15`).

**2b. «Ti sembrano uguali?» diventa una scelta**
- `maes_16` si ferma dopo «Ti sembrano uguali?».
- **Interazione**: indice = sì, medio = no.
- **Risposte (bozza)**: se sì, «Ascolta bene…» e si fa ripetere il confronto; se no, «Esatto! Sono diversi!». Poi riprende «Anche questa volta ho battuto le mani tre volte…».

**2c. Fare il ritmo del Maestro**
- Dopo `maes_19` (TAC, TAC… TOC):
- **Nuova battuta (bozza)**: «Vuoi provare a fare il mio ritmo? Due colpi vicini… e uno dopo una piccola pausa. Prova!»
- **Interazione**: TAC TAC… TOC con il pulsante.
- Poi `sfx_18` e `maes_20`, con il pianoforte che imita, cambiata così (bozza): «Hai sentito? Anche il pianoforte ha voluto rifarlo!»

**Da accorciare nello stesso blocco**:
- La seconda metà di `maes_16` (da «Questo è uno dei modi…» a «…ripeterli.») è lunga e la dimostrazione con le mani la dice già.
- I passi (`sfx_15` – `maes_18`) sono belli per un bambino cieco (i passi li conosce bene) e li terrei, eventualmente più corti.

Da registrare: circa 5–6 battute brevi. Da programmare (Tommi): un'interazione "ripeti il ritmo" (anche una versione molto semplice va bene per ora: conta le pressioni e guarda solo se le distanze sono più o meno giuste).

---

## Proposta 3: la nota che manca si trova premendo

Il blocco del pianoforte mostra la nota mancante **tre volte in meno di un minuto** (`sfx_24`, `sfx_26`, `sfx_27`). Proposta:
- Togliere `sfx_24`, che arriva prima del ricordo, così l'ordine è chiaro: prima com'era (`sfx_25`), poi com'è adesso (`sfx_26`).
- Dopo `maes_27` («…senti cosa succede»):
  - **Nuova battuta (bozza)**: «Quando pensi che dovrebbe arrivare la terza nota… premi!»
  - **Interazione**: il pianoforte suona le due note; il bambino preme nel momento in cui dovrebbe esserci la terza.
  - **Risposta (bozza)**: «Proprio lì! È lì che manca il suono.»
- Questo prepara anche la Scena 3, dove il bambino deve ripetere il tema.
- `maes_28` e `maes_29` («C'era una volta un piccolo…», «Un panino?!») si tengono: sono divertenti e li hai voluti tenere.
- `sfx_27` (il pianoforte che riprova) si può togliere, oppure lasciarlo solo sotto «Oh, povero pianoforte…».

---

## Proposta 4: «Ti andrebbe di aiutarmi?» ha una risposta

`maes_33` finisce con una domanda vera, ma il bambino non può rispondere. Proposta:
- **Interazione**: «Se vuoi aiutarmi, premi l'indice!». È la stessa meccanica del Tocco, quindi funziona solo con la Proposta 1.
- **Risposta**: `maes_34` comincia già con «Aspetta, aspetta! Prima che tu risponda…». Va cambiata in qualcosa come (bozza) «Evviva! Lo sapevo!», oppure si toglie l'inizio.

---

## Proposta 5: tagli (non serve registrare niente)

| Battuta | Cosa togliere | Perché |
|---|---|---|
| `maes_23` | da «Sai, normalmente il vento non mi dispiace…» fino a «…ricominciare da capo!» (resta «Sbuffo. Quel vento dispettoso.» e, se piace, «Sbuffa quando è contento… Sbuffa ancora più forte!») | l'aneddoto dei fogli è carino ma lungo, e i fogli tornano subito in `maes_25` |
| `maes_24` | «Hai ascoltato la musica all'inizio? … Era tutto pronto.» | il bambino l'ha appena sentito nell'Intro |
| `maes_30` | «È successo così in fretta…» (l'ultima frase rimasta) | si capisce già |
| `maes_34` | «Non devi essere un musicista… in vita tua.» (resta «Potremo scoprire tutto insieme…») | lista lunga, e il messaggio si capisce in una frase |
| `maes_35` | «A volte dovremo riconoscere un suono… suonare insieme. Ogni volta che aiuteremo uno strumento, recupereremo un nuovo pezzo della nostra canzone.» | lo scoprirà giocando; resta il bel finale musicale «E piano piano… un suono alla volta…» |
| `maes_38`, `sfx_37`, inizio di `maes_39` | la gag del pianoforte scomodo | con la Proposta 1 il Tocco arriva prima e la gag non regge più |

`maes_37` (perché Sbuffo ha rubato la musica?) la terrei: è il "mistero" che tiene viva la storia per le scene dopo, e dura poco.

---

## Proposta 6: Intro (facoltativa)

L'Intro è un racconto di 5 minuti con musica e suoni: come "film d'apertura" può reggere. Due piccole idee:
- **Un primo gesto semplicissimo** all'inizio. Il Narratore, prima di `p00_narr_01` (bozza): «Prima di cominciare… tieni in mano il Tocco. Quando sei pronto ad ascoltare, premi un pulsante qualsiasi.» Senza spiegare le dita: quello lo fa poi il Maestro. Il bambino sa da subito che il gioco "lo aspetta".
- **`p00_narr_01` un po' più corta**: la parte «Alcune ti fanno venire voglia di correre. Altre… E altre ancora…» si può accorciare, perché arriva prima che succeda qualcosa.

---

## Riassunto: come verrebbe la Scena 1

| Circa al minuto | Cosa succede | Il bambino |
|---|---|---|
| 0:00 | Arriva il Maestro, bacchetta, si presenta, TAC TAC TOC | ascolta |
| 2:30 | **Il Tocco** | **preme l'indice** |
| 3:30 | Il problema, gli strumenti insieme | ascolta |
| 4:30 | **Il ritmo con le mani** | **ripete TA TA TA**, **risponde «sono uguali?»** |
| 6:00 | Passi, poi il ritmo del Maestro | **fa TAC TAC TOC** |
| 7:00 | Sbuffo (corto) | ascolta |
| 8:00 | **La nota che manca** | **preme dove manca la nota** |
| 9:00 | Il Maestro chiede aiuto | **risponde sì** |
| 10:00 | Il viaggio (corto), la scelta finale | **sceglie** (c'è già) |

Durata stimata **circa 10–11 minuti** invece di 16, con **6 interazioni** invece di 2.

## Cosa serve per farlo

- **Bianca**: decidere quali proposte tenere, scrivere le battute nuove (circa 10–12 frasi brevi) e registrarle con la voce di Antonio Farina. I tagli si fanno sull'audio che c'è già.
- **Tommi**: le interazioni nuove. "Premi un pulsante" e "scegli tra due" il gioco le sa già fare. Mancano "ripeti il ritmo" (2a, 2c) e "premi al momento giusto" (3), che sono proprio la meccanica principale del gioco: conviene farle insieme.
- **Poi**: copione aggiornato, rigenerare i CSV, tagliare l'audio, regia e scena (come per i tagli delle ripetizioni).
