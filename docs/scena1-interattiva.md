# Scena 1 interattiva

Versione montata l’11 novembre 2026: **93 elementi**, 45 battute narrative,
40 suoni e otto punti di interazione, di cui uno nel ramo di riascolto.
Gli audio del montaggio e della scelta di riprovare sono presenti.
Restano il riascolto in cuffia e,
quando sarà disponibile, la verifica del controller fisico.

## Le cinque modifiche approvate

1. Tocco viene presentato subito dopo «Quello sono io!».
2. Il bambino ripete tre battiti regolari e poi TAC TAC… TOC.
3. «Ti sembrano uguali?» diventa una domanda con risposta Sì/No.
4. Si preme nel momento della terza nota mancante.
5. Il Maestro aspetta una risposta alla richiesta d’aiuto.

Sono sei momenti di partecipazione prima della scelta finale, perché il secondo
punto comprende due esercizi. La scelta finale tra andare al pianoforte e
riascoltare è conservata. L’Intro non ha nuove interazioni.

## Dialogo e registrazioni

Sì e No vengono spiegati **una volta sola**, nel tutorial di Tocco. Sono i due
pulsanti del pollice, separati dai quattro pulsanti delle note. In seguito
si fanno domande naturali, senza ripetere «premi Sì… premi No…»:

- «Ti sembrano uguali?».
- «Ti andrebbe di aiutarmi a recuperare la nostra musica?».
- «Vuoi andare subito a conoscere il pianoforte?».

Il riascolto dopo un tentativo non rilegge la consegna. R permette di
richiederla esplicitamente. Nessuna domanda viene ripetuta automaticamente
perché il bambino non risponde. No alla richiesta d’aiuto apre un’attesa calma;
si continua con Sì, senza conto alla rovescia o richiami insistenti. Sono pronti
il codice e i [testi di quattro inviti alternativi](registrazioni-scena1-inviti.txt)
da attivare dopo aver ricevuto i nuovi audio. Ogni nuovo No farà ascoltare
l’invito successivo; il silenzio continuerà a non provocare ripetizioni.

Per il montaggio dell’11 novembre sono stati controllati 20 MP3 nei Downloads:
17 contenuti distinti e tre copie identiche. Il 12 novembre sono stati aggiunti
tre audio per la scelta di riprovare e le risposte alternate. I file originali
sono conservati in `Story/Audio/Interazioni/_ricevuti/`;
i messaggi pronti sono in `Story/Audio/Interazioni/`. Le versioni estese delle
istruzioni del confronto e dell’aiuto restano archiviate. La risposta d’attesa
è tagliata dopo «Io resto qui», prima del comando finale «premi Sì».

Le domande del confronto e dell’aiuto si ricavano dalle voci già registrate.
Tutte le nuove battute restano con **Antonio Farina – Expressive and Warm**;
non servono altre voci. Il raccordo dopo il tutorial riutilizza la seconda
parte di `s01_maes_10` della versione precedente.

Riferimenti:

- [Testi attivi](registrazioni-scena1-interattiva.csv).
- [Inventario delle registrazioni](registrazioni-scena1-ricevute.json).
- [ID precedenti e correnti, tagli e configurazione](scena1-montaggio.json).
- [Stato delle consegne audio](audio-da-completare-scena1.md).

## Comandi della prova

| Comando | Funzione |
|---|---|
| `1` | Nota sotto l’indice: prova iniziale, ritmo e nota mancante |
| `2`–`4` | Altri pulsanti musicali; non rispondono alle domande |
| `S` oppure `Y` | Sì |
| `N` durante un’interazione | No |
| `N` durante il parlato | Salta, come comando di prova |
| `R` | Ripete l’elemento corrente, compresa la consegna se richiesta |
| `Spazio` | Pausa; durante un’attesa di scelta conferma il primo pulsante valido |
| `Backspace` | Ricomincia la sezione |

I due numeri HID fisici del pollice non sono documentati nel protocollo:
non si presume che siano 5 e 6. `yesThumbButton` e `noThumbButton` nell’Inspector
restano a zero, non associati, finché si proverà il controller. S/Y e N funzionano
indipendentemente da questa associazione. I valori logici 5 e 6 usati dalla
regia significano Sì e No e non sono numeri di cablaggio.

## Punti di prova nella scena

| Elemento | ID | Cosa verificare |
|---|---|---|
| 25/93 | `s01_int_01` | Primo gesto musicale: l’indice produce il segnale e il Maestro risponde |
| 36/93 | `s01_int_02` | Tre colpi regolari, con feedback a ogni pressione |
| 37/93 | `s01_int_03` | Due esempi, domanda breve; Y riascolta e N riconosce che sono diversi |
| 45/93 | `s01_int_04` | TAC TAC… TOC; il pianoforte imita dopo il tentativo riuscito |
| 57/93 | `s01_int_05` | Premere dove dovrebbe esserci la terza nota |
| 67/93 | `s01_int_06` | Domanda d’aiuto: N aspetta tranquillamente, Y continua |
| 80/93 | `s01_int_07` | Y va al pianoforte, N riascolta la canzone |
| 84/93, solo riascolto | `s01_int_08` | Domanda breve dopo il ricordo: Y prosegue, N aspetta |

La descrizione di Sbuffo resta; il racconto del furto già tolto non ritorna.
«Un panino?!» rimane. La gag del pianoforte scomodo non precede più Tocco.
Le indicazioni ripetute sui pulsanti e le domande retoriche nei punti tagliati
non sono lette nuovamente.

## Dati e riproducibilità

### Due tentativi, poi una scelta

La logica permette due tentativi per volta negli esercizi di ritmo,
confronto e nota mancante. Dopo due esiti non riusciti il Maestro chiede
«Ti va di riprovare?». Sì apre un altro gruppo di due tentativi; No fa
ascoltare un incoraggiamento e prosegue nella storia. La rinuncia non viene
presentata come un successo e non cambia il ramo narrativo. Il silenzio
durante la domanda non provoca richiami automatici.

La scelta è configurata per `s01_int_02`, `s01_int_03`, `s01_int_04` e
`s01_int_05`. I tre nuovi audio ricevuti il 12 novembre sono importati con
gli originali MP3 conservati in `_ricevuti`. `eq_s01_extra_15_ancora` è la
domanda; le risposte `eq_s01_extra_16_migliorare` e
`eq_s01_extra_17_migliorare_alt` si alternano fra le rinunce ai diversi
esercizi. Il riavvio della storia ricomincia dalla prima risposta.
I [testi registrati](registrazioni-scena1-riprova.txt) restano disponibili
in formato leggibile, con i toni fra parentesi quadre. Non servono altri audio.

Nella nota mancante una pressione troppo anticipata non impedisce più di
correggersi premendo nel momento giusto durante lo stesso ascolto. La finestra
conclusa senza pressioni conta come una prova nella modalità con limite.
R e il riavvio azzerano il conteggio; il tasto 1 funziona anche dal tastierino numerico.

### Configurazione del montaggio

Il copione Markdown resta la fonte della sequenza narrativa. I quattro CSV
si rigenerano con `tools/story_extract.py`; la regia è scritta a mano. I
messaggi condizionali sono un banco audio separato: non sono duplicati come
battute automatiche nella sequenza. La mappa di montaggio conserva i vecchi
ID, i nuovi ID e i punti dei tagli. I GUID dei file riutilizzati sono conservati.

La regia supporta:

| Campo | Contenuto |
|---|---|
| `modalita` | `conferma`, `ritmo`, `confronto`, `nota`, `scelta`; vuoto conserva il comportamento precedente |
| `consegna` | Nome del messaggio senza estensione oppure ID voce; letto una volta |
| `successo` | Risposta dopo l’esito riuscito, facoltativa |
| `riprova` | Invito calmo a riascoltare nei tentativi di ritmo/nota/confronto |
| `esempi` | Un nome per ritmo/nota, due per confronto, separati da `;` |
| `tempi` | Tre istanti in secondi; esempio `0;0.32;0.95` |
| `suono_pulsante` | Feedback breve facoltativo sulle pressioni |
| `pulsante` | `indice` per la prima prova musicale; conferme altrimenti sul Sì del pollice |
| `attesa_no` | Un messaggio accoglie No una sola volta; più messaggi separati da `;` avanzano ciclicamente a ogni nuovo No, aspettando poi Sì/No in silenzio |
| `scelte` | Nelle nuove scelte `5=B;6=A`: Sì al ramo B, No al ramo A |
| `tentativi` | Facoltativo: da 1 a 3, previsto 2; richiede entrambi gli audio seguenti |
| `domanda_riprova` | Domanda Sì/No dopo il numero di tentativi indicato |
| `prosegui` | Incoraggiamenti separati da `;`, alternati dopo No; poi prosegue senza la risposta di successo |

Il confronto ritmico è locale: non cambia il ramo narrativo. Le pressioni
sono raccolte dai timestamp di `InputSystem.onEvent`, evitando di perdere tre
pressioni avvenute nello stesso aggiornamento. L’offset verso l’orologio DSP
viene fissato all’apertura dell’attesa e ricalcolato dopo una pausa.
Anche il pulsante Pausa dell’Editor ricalcola questa corrispondenza alla
ripresa: l’orologio degli eventi continua durante la pausa, quello audio si
ferma. La prova con il file reale della nota mancante falliva prima della
correzione e ora deve riuscire anche dopo una pausa dell’Editor.

Durante l’esercizio della nota mancante si usa `1`, anche dal tastierino:
il bersaglio è 1,5 secondi dopo l’inizio del pianoforte, subito dopo la
seconda nota a 1,2 secondi. Alla domanda «Ti va di riprovare?» invece si
risponde S/Y oppure N: i tasti musicali non rispondono alle domande.

Tempi degli esempi: battiti regolari 0, 1, 2 secondi; Maestro 0, 0,32, 0,95;
tema della canzone 0, 1,2, 1,5. L’esempio della nota mancante usa i tempi esatti
del tema completo, senza la terza nota. La pressione non ne ripristina il suono.
I cinque esempi/feedback `x_s01_*` si rigenerano col generatore dei suoni.

I ritmi accettano un tempo globale tra metà e doppio dell’esempio e uno scarto
per intervallo fino al maggiore tra 0,10 secondi e il 30% dell’intervallo atteso.
Tre pressioni regolari non passano come TAC TAC… TOC. La nota mancante ha
una tolleranza di 0,30 secondi, senza aprire la finestra prima di 0,05 secondi
dopo la seconda nota. Sono valori da calibrare con l’ascolto e il controller,
non soglie validate con bambini.

La programmazione usa `AudioSettings.dspTime` e `AudioSource.PlayScheduled`;
`AudioListener.pause` congela anche questo orologio. Fonti:
[PlayScheduled](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/audiosource/playscheduled),
[AudioListener.pause](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/audiolistener/pause).

Le battute narrative e le consegne delle interazioni aspettano la fine della
riproduzione effettiva prima di proseguire. Anche le pause fra gli elementi
usano l’orologio audio. Il precedente conteggio con `unscaledDeltaTime`, dopo
una pausa dell’Editor, poteva far partire la battuta successiva mentre la prima
era ancora in corso: riprodotto con due voci simultanee dopo soli 0,149 secondi
di una registrazione di due secondi. Non è necessario rigenerare gli audio.
Gli anticipi espliciti degli effetti sonori sulla coda della voce restano nella regia.

## Verifiche

Da `unity/EchoQuestTest`, con l’Editor collegato:

```sh
unity command 'script run' --file ../../tools/story_interaction_checks.cs --entry StoryInteractionChecks.Run
```

In Play in una scena temporanea vuota, poi ripristinando la scena originale:

```sh
unity command 'script run' --file ../../tools/story_audio_checks.cs --entry StoryAudioChecks.Run --timeout 40
unity command 'script run' --file ../../tools/story_interaction_runtime_checks.cs --entry StoryInteractionRuntimeChecks.Run
unity command 'script run' --file ../../tools/story_scene_checks.cs --entry StorySceneChecks.Run --timeout 180
unity command 'script run' --file ../../tools/debug_overlay_checks.cs --entry DebugOverlayChecks.Run --timeout 60
```

Prima di entrare in Play con l'Editor non in primo piano: `unity command editor.playmode set_run_in_background`
(va ripetuto dopo ogni ricompilazione), altrimenti il gioco resta fermo al primo fotogramma e le prove vanno in timeout.

Esiti dell’11–12 novembre:

- Compilazione senza errori.
- Riprodotta e corretta la sovrapposizione dopo una pausa dell’Editor: controllo sulla narrazione e sulle consegne divise in più parti.
- 52 controlli sulle regole e sulla configurazione superati.
- 43 controlli in Play: note/risposte separate, No, pausa, ripeti, riavvio, tempi, pressione anticipata corretta, tastierino numerico, limite dei tentativi, risposte alternate, inviti dopo No, alias S e prova della nota con il file audio reale prima e dopo la pausa dell’Editor.
- Percorso diretto completato con sette interazioni; percorso di riascolto con otto.
- Completato anche il percorso che rinuncia a tutti e quattro gli esercizi dopo due tentativi, con sette interazioni e arrivo allo stesso finale.
- Verificati 93 elementi, 40 suoni di scena, cinque esempi/feedback, riferimenti audio e GUID riutilizzati.
- Superati i tre test Python delle pause vocali.

Le prove complete usano il copione e la regia reali, accorciando e silenziando
la narrazione per verificare il percorso senza un ascolto integrale. Le prove
dell’input usano una tastiera virtuale; il ramo B iniziale del confronto viene
impostato come stato della fixture. Restano da verificare qualità dei raccordi,
riverbero e comprensibilità in cuffia, oltre al futuro controller fisico.
