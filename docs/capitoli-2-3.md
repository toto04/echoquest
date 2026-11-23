# Capitoli 2 e 3: il tamburo e la campanella, il menu

Versione del 19 novembre 2026. Stato: **copioni, musica, voci e suoni pronti**; non ancora in Unity
(regia, scene e le nuove modalità sono da fare, vedi [Cosa costruire](#cosa-costruire)).

- Copioni: `Story/Script/capitolo_02_il_tamburo.md` e `Story/Script/capitolo_03_la_campanella.md`.
- Musica: `music/library/capitolo_02_tamburo.yaml` e `music/library/capitolo_03_campanella.yaml`,
  controllate con `python3 -m tools.music.check` (0 errori).
- Regole seguite: [`docs/music/`](music/README.md), in particolare la scala degli stadi di
  [02-calls.md](music/02-calls.md#stage-ladder), le fasi di una sfida di
  [04-battle-cues.md](music/04-battle-cues.md#phases-of-one-battle) e le schede dei personaggi.

## Come si passa da un capitolo all'altro

```
avvio ─► menu ─► Capitolo 1 (pianoforte) ─► Scena 5: tamburo o campanella?
                                              │
                        Epilogo: «Vuoi andare subito dallo strumento che hai scelto?»
                          Sì ─► il capitolo scelto ─► Epilogo: «Vuoi andare dal prossimo?»
                          No ─► menu                    Sì ─► l'altro capitolo   No ─► menu
```

- Il **tamburo** e la **campanella** sono capitoli separati, uno per strumento. La scelta della Scena 5
  decide quale si gioca per primo; l'altro si sblocca insieme, e si può scegliere dal menu.
- Per questo **ogni capitolo funziona in tutti e due gli ordini**: nessuna battuta dà per ritrovato
  l'altro strumento. Solo la musica se ne accorge: la ripresa finale suona la canzone *fino ai pezzi
  ritrovati*, e nelle sfide uno strumento già ritrovato diventa uno strato in più della base
  ([04](music/04-battle-cues.md#adaptive-layers-vertical-layering): «each instrument the child has won
  back becomes a permanent extra stem»).
- Dopo tamburo e campanella viene il **flauto** (capitolo 4, da scrivere): l'ultima tappa annunciata
  nell'epilogo del capitolo 1, dove i due indizi su Sbuffo si uniscono.

### Il menu (solo audio)

I bambini non vedono lo schermo: il menu è il Maestro che parla, e si sceglie con le dita, come le scelte
della storia. Battute da registrare (proposta):

| Momento | Battuta del Maestro |
|---|---|
| Benvenuto | «Bentornato nella sala della musica! Che cosa vuoi fare?» |
| Indice | «Per la prima avventura, la canzone rubata dal vento, premi l'indice.» |
| Medio, aperto / chiuso | «Per il tamburo, premi il medio.» / «Il tamburo ti aspetta dopo la prima avventura.» |
| Anulare, aperto / chiuso | «Per la campanella, premi l'anulare.» / «La campanella ti aspetta dopo la prima avventura.» |
| Mignolo | «Per il quaderno delle canzoni, premi il mignolo.» |
| Sì | ripete le scelte. Nel silenzio il Maestro aspetta, senza richiami. |

- **Il quaderno delle canzoni** è la «collezione» proposta dalla relazione sulla musica: ogni pezzo
  ritrovato (la frase del pianoforte, il galoppo, la domanda e la risposta della campanella) si può
  riascoltare e suonare liberamente sopra la sua base.
- Il gioco ricorda i progressi (capitoli finiti, scelta della Scena 5, registro del pianoforte) tra una
  sessione e l'altra.
- Al capitolo 1 va aggiunta l'ultima domanda dell'epilogo: «Lo strumento che hai scelto ti sta già
  aspettando. Vuoi andare subito da lui? Premi Sì per partire, oppure No per tornare al menu.»
  Si aggiunge al copione insieme al menu, perché senza menu non avrebbe dove andare.

## La scala di difficoltà

Il capitolo 1 si ferma allo stadio 2 (eco con ritmo relativo, scambi lunghi, tre dita). I capitoli 2 e 3
partono da lì e arrivano allo stadio 3, con un **livello extra facoltativo** allo stadio 4. Ogni capitolo,
da solo, ha più interazioni di tutto il capitolo 1, e al massimo circa 75 secondi di parlato tra
un'interazione e l'altra.

| | Capitolo 1: pianoforte | Capitolo 2: tamburo | Capitolo 3: campanella |
|---|---|---|---|
| Che cosa si impara | il suono, grave e acuto, piano e forte | il passo, il tempo, il ritmo | durata, spazio, domanda e risposta |
| Interazioni | 20 (dall'Intro all'Epilogo) | 22 | 27 |
| Dita | indice, medio, anulare | + **mignolo** | + **mignolo** |
| Registro dei pulsanti | scelto (grave o acuto) | un'ottava sotto (Re3-Do4) | un'ottava sopra (Re5-Do6) |
| Stadi | 0 → 2 | 0 → 3, extra 4 | 0 → 3, extra 4 |
| Scambi nella sfida | lunghi | lunghi, poi **corti** | lunghi |
| Novità della sfida | | aiuto dopo 2 errori, frase accorciata dopo 3, cambio di base (Re → Fa), turno d'attacco da memoria, **colpo finale** a tempo | **risposta** invece dell'eco, **risposta libera** inventata, **rintocco finale** a tempo |
| Centro | Re (sfida), Fa (vittoria) | Re, poi Fa al turno 4 («l'hai domato») | Fa |

### Capitolo 2, il tamburo: le interazioni in ordine

| Scena | Interazione | Modalità | Stadio |
|---|---|---|---|
| 1 | suonare i tamburi liberamente (misura il tempo del bambino) | libero | 0 |
| 1 | il mignolo | conferma | — |
| 1 | «Il secondo è regolare come il primo?» (No) | confronto | — |
| 2 | batti il tempo con l'orchestra (6 colpi su 8) | **battito** (nuova) | 1 |
| 2 | batti il tempo da solo quando la musica si fa piccola (5 su 8) | **battito** | 2 |
| 2 | «Il secondo è più veloce?» (Sì) e poi (No) | confronto ×2 | — |
| 3 | parole del tamburo: *me-la*, *co-li-brì*, *tar-ta-ru-ga* | eco di allenamento ×3 | 1-2 |
| 3 | *co-li-brì* su due tamburi (La La Re) | eco di allenamento | 2 |
| 4 | «Sono uguali?» il galoppo vero e quello al contrario di Sbuffo (No) | confronto | — |
| 4 | sfida, turni 1-4: Re Re Re → due volte → La La Re (corto) → il galoppo La La Do La La Fa (corto, su Fa) | sfida | 2 → 3 |
| 5 | turno d'attacco: la frase del pianoforte da memoria | sfida senza chiamata | 2 |
| 5 | colpo finale sul «BUM!» | **colpo** (nuova) | — |
| 5 | battere il tempo con Sbuffo, che piano piano soffia a tempo | libero | 0 |
| 5 | livello extra? Sì/No, poi il ritmo 3+3+2 | scelta, eco | 4 |
| Epilogo | vai al prossimo strumento? Sì/No | scelta | — |

### Capitolo 3, la campanella: le interazioni in ordine

| Scena | Interazione | Modalità | Stadio |
|---|---|---|---|
| 1 | suonare le campanelle liberamente | libero | 0 |
| 1 | «Il secondo suono è più corto?» (Sì) | confronto | — |
| 1 | lungo o corto? (indice / medio) ×2 | **riconosci** (nuova) | — |
| 2 | dove suona? sinistra o destra (indice / medio) ×3, l'ultima in movimento | **riconosci** | — |
| 2 | quante volte suona? 2, poi 3 | **conta** (nuova) | — |
| 3 | il mignolo | conferma | — |
| 3 | *Ciao* (Do La), *din don din* (La Do La) | eco di allenamento ×2 | 1 |
| 3 | «La seconda frase ti sembra finita?» (Sì) | confronto | — |
| 3 | l'altalena (Fa La Fa), la frase del bambino | eco di allenamento | 1 |
| 4 | risposta fissa: la campanella chiede, il bambino risponde con l'altalena ×2 | sfida con risposta | 1 |
| 4 | il periodo: domanda Fa La Do La Do, risposta Fa La Do La Fa | sfida con risposta | 3 |
| 4 | la risposta rubata (La Do Fa), dopo il ricordo | eco di allenamento | 2 |
| 5 | gioco con Sbuffo, turni 1-3: due eco, poi la risposta rubata | sfida | 2 → 3 |
| 5 | turno d'attacco: risposta libera, basta che finisca sul medio | **risposta libera** (nuova) | 3 |
| 5 | rintocco finale | **colpo** | — |
| 5 | rispondere alla domanda di Sbuffo (non c'è risposta sbagliata) | risposta libera, senza giudizio | — |
| 5 | livello extra? Sì/No, poi la ninna nanna in tre | scelta, eco | 4 |
| Epilogo | vai al prossimo strumento? Sì/No | scelta | — |

## La musica in breve

Tutte le regole del [cartellino](music/README.md#the-rules-card) valgono anche qui; queste sono le scelte
dei due capitoli.

- **Il pezzo di canzone di ogni strumento** viene da [06-worked-example.md](music/06-worked-example.md):
  il tamburo riconquista le battute 3-4 (il galoppo, `A A C - A A F -`), la campanella le battute 7-8
  (la domanda `C A C A C - A -` e la risposta `A - C - F - - -`). Ogni chiamata della sfida è un pezzo,
  una variazione o una versione «scombinata da Sbuffo» di quel pezzo.
- **Prima il ritmo, poi le note** (02): il tamburo insegna *co-li-brì* su un tamburo solo, poi su due,
  poi lo mette sulle note del galoppo. Le parole italiane hanno l'accento sulla nota lunga, come chiedono
  i documenti (*co-li-BRÌ*, *tar-ta-RU-GA*).
- **Una sincope si sente prima nella base e solo dopo in una chiamata** (02): il 3+3+2 entra nel basso
  quando la musica torna a casa, e solo il livello extra lo chiede al bambino. Se il bambino lo
  «raddrizza», non è un errore: è memoria normale, e il Maestro lo aiuta.
- **Gli scambi corti** (la frase una volta sola) arrivano dal turno 3 del tamburo, come raccomanda
  [07, D3](music/07-open-questions.md): lunghi agli stadi 1-2, corti dallo stadio 3.
- **Le domande finiscono aperte, le risposte tornano a casa** (02, menu delle cadenze): è il cuore del
  capitolo della campanella. La risposta libera vale se l'ultima nota è il Fa.
- **Registri**: il tamburo suona un'ottava sotto, la campanella un'ottava sopra (04, schede); le basi
  sono scritte per quei registri, con la zona della chiamata sempre libera.
- **Il cambio Re → Fa sotto la stessa frase** è il segnale «l'hai domato» (01): nel tamburo succede
  prima dell'ultimo turno, così il galoppo vero arriva sulla base di casa.
- **Gli errori** seguono [04, Failure](music/04-battle-cues.md#failure-musically): la presa in giro
  comica, poi la base d'aiuto, poi la frase accorciata; mai più lenta, e dopo tre tentativi il Maestro
  offre di suonarla insieme.

Il checker segnala solo avvisi W8 (due frasi che differiscono per un simbolo di profilo):
*co-li-brì* e *due tamburi* (voluto: è lo stesso ritmo con note nuove, dentro la stessa sfida), e, fra
capitoli diversi, *me-la* con *Ciao* e *due tamburi* con la domanda di Sbuffo, che stanno su strumenti e
registri diversi.

## Cosa costruire

In ordine, ogni passo verificabile da solo:

1. ~~**ID dei capitoli.**~~ Fatto: `tools/story_extract.py --section-letter t` (o `c`) dà `t00`, `t01`...,
   `t99` per l'epilogo. Lo `StoryPlayer` accetta già qualsiasi lettera (`[a-z]\d\d`).
2. **Menu e progressi**: la scena del menu, il salvataggio dei progressi, il passaggio da un capitolo
   all'altro, la domanda finale nell'epilogo del capitolo 1, il quaderno delle canzoni.
3. **Modalità nuove** (regole in `StoryInteractionRules.cs`, con i controlli in
   `tools/story_interaction_checks.cs`):
   - `battito`: battere il tempo sopra una base; un colpo vale se è entro una finestra dal movimento,
     la prova riesce con una quota di movimenti presi;
   - `riconosci`: come il confronto, ma la risposta giusta è un pulsante delle dita (lungo/corto,
     sinistra/destra);
   - `conta`: conta solo il numero delle pressioni;
   - `risposta libera`: almeno due note, l'ultima sulla nota di casa, dentro la battuta;
   - `colpo`: una pressione sola, vicina a un movimento preciso.
   La risposta fissa e il periodo non hanno bisogno di una modalità nuova: nella `sfida` la frase che
   suona (`esempi`) e quella attesa (`pulsanti`, `tempi`) sono già separate.
4. **Sfida**: scambi corti; cambio di base sul confine di uno scambio; base d'aiuto dopo due errori e
   frase accorciata dopo tre; offerta di aiuto dopo tre tentativi; turno d'attacco senza chiamata;
   strumenti ritrovati come strati della base; timbro e registro dei pulsanti per capitolo.
5. ~~**Voci**~~ Fatto: 115 registrazioni (73 battute e 42 risposte), testi in
   `docs/registrazioni-capitoli-2-3.txt`. Mancano le battute del menu.
6. ~~**Suoni e musica**~~ Fatto: tamburo e campanella sono timpani e campane a mano veri (VCSL, CC0),
   accordati sulle note dei pulsanti; frasi, basi a strati, esempi e suoni di scena in `Audio/Sfx`
   (`t…`, `c…`, `x_t…`, `x_c…`). Restano regia, scene Unity e controlli in Play mode.
7. **Capitolo 2 in Unity, scena per scena.** Regia in `Story/Script/capitolo_02_regia.csv` (stesse colonne
   del capitolo 1), scene in `Story/Scenes/Tamburo/` (menu *EchoQuest → Rebuild Tamburo Scenes*), controlli
   in Play mode in `tools/story_tamburo_runtime_checks.cs` (`TamburoRuntimeChecks.RunSceneN`). Ogni scena
   contiene tutto il capitolo e prosegue nella successiva, come nel capitolo 1.
   - [x] Prologo e Scena 1: suono libero sui tamburi (il gioco misura il tempo spontaneo del bambino,
     `StoryPlayer.SpontaneousBeatSeconds`), prova del mignolo (`pulsante`: `anulare` e `mignolo`),
     confronto «regolare?».
   - [x] Scena 2: `battito` (tenere il passo: `tempi` = i movimenti da valutare, `quota`, `tolleranza`,
     `aiuto` = il Maestro che conta «uno, due, tre, quattro» sui primi movimenti dopo un errore,
     `x_t02_conta_1…4` tagliati dalla registrazione) e i due confronti di tempo.
   - [ ] Scena 3: eco di allenamento senza Sbuffo.
   - [ ] Scena 4: sfida con scambi corti, cambio di base, aiuto dopo gli errori.
   - [ ] Scena 5: turno d'attacco, `colpo`, il vento a tempo, livello extra.
   - [ ] Epilogo.

## Domande per il gruppo

- **La risposta libera** (campanella): basta «l'ultima nota sul medio», o per i più piccoli basta che
  arrivi una risposta qualsiasi? Va provato con i bambini.
- **Il gioco «dove suona»** funziona solo con cuffie o altoparlanti stereo: lo chiediamo a un adulto
  all'inizio, nelle impostazioni?
- **Le finestre di tempo** di `battito` e `colpo` (150 e 200 ms) sono i valori di partenza della relazione
  sulla musica per 5-7 anni: vanno provate con il controller vero, perché dipendono dalla latenza.
- **Che cosa sa il bambino quando apre il menu la prima volta**: il menu parte subito, o la prima volta
  si va dritti al capitolo 1?
