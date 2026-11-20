# Scene 2, 3, 4, 5 ed epilogo interattivi

Versione del 19 novembre 2026. Le scene si giocano in Unity
(`Story/Scenes/Scena2.unity`, `Scena3.unity`, `Scena4.unity`, `Scena5.unity` ed `Epilogo.unity`), con tutte
le voci, i suoni e la musica. Il capitolo si gioca di fila, senza stacchi:
Intro → Scena 1 → Scena 2 → Scena 3 → Scena 4 → Scena 5 → Epilogo. Ogni scena Unity contiene tutto il
capitolo e dice solo da dove partire; alla fine di una sezione il lettore passa alla successiva senza caricare
un'altra scena, così i suoni che stanno ancora risuonando non si interrompono. Nel pannello di debug (scheda
Story) ci sono un pulsante per scena e l'interruttore «Play on into the next scene»: spento, si ascolta
solo la scena scelta.
Il copione è in `Story/Script/capitolo_01_la_canzone_rubata_dal_vento.md`; le regole
musicali sono quelle di [`docs/music/`](music/README.md).

## Come si gioca

Ogni scena alterna poco parlato e un'interazione (al massimo circa 75 secondi senza interazione).
I quattro pulsanti delle note hanno sempre la stessa altezza: indice Re, medio Fa, anulare La,
mignolo Do. Sì e No sono i due pulsanti del pollice.

| Scena | ID | Interazione | Cosa conta |
|---|---|---|---|
| 2 | `s02_int_01` | premi l'indice: suona Re4 del pianoforte | una pressione dell'indice |
| 2 | `s02_int_02` | «Sono le stesse note?» (piano e forte) | la risposta giusta è **Sì** |
| 2 | `s02_int_03` | grave o acuto | indice = grave, medio = acuto; nessuna risposta sbagliata |
| 2 | `s02_int_05` | il ritmo del Maestro sul pianoforte | TAC, TAC... TOC, gli stessi tempi della Scena 1 |
| 3 | `s03_int_01` | premi il medio: suona Fa | solo il medio conferma |
| 3 | `s03_int_02` | sfida, turno 1: Re... Re-Re | ordine e ritmo relativo dopo il TOC |
| 3 | `s03_int_03` | sfida, turno 2: Re... Fa-Re | come sopra, con il medio in mezzo |
| 3 | `s03_int_04` | sfida, turno 3: La... Fa-Re | anulare, medio, indice, con lo stesso ritmo |
| 4 | `s04_int_01` | suono libero con pianoforte e campanella | qualsiasi tasto, in qualsiasi momento: nessuna nota sbagliata |
| 4 | `s04_int_02` | «Secondo te Sbuffo vuole solo fare i dispetti?» | Sì o No, nessuna risposta sbagliata |
| 5 | `s05_int_01` | tamburo (a sinistra) o campanella (a destra)? | indice = tamburo, medio = campanella; i pollici non scelgono; nessuna scelta sbagliata |
| epilogo | `e00_int_01` | l'ultimo TA... TA-TA, come uno scambio della sfida | niente si conta: ogni pulsante suona, e anche il silenzio va bene |

Le consegne sono dette dalle battute del Maestro subito prima dell'interazione, quindi la
regia lascia vuota la colonna `consegna`: al primo tentativo si ascolta subito. Dopo due tentativi
non riusciti il Maestro chiede «Ti va di riprovare?», come nella Scena 1.

### Il registro (Scena 2)

La scelta sposta **tutti e quattro i pulsanti** di un'ottava (01-palette.md): grave = Re3–Do4,
acuto = Re5–Do6. Da quel momento i pulsanti, gli esempi e la sfida suonano nel registro scelto.
La scelta resta valida nelle scene successive; chi parte direttamente dalla Scena 3 usa
`defaultRegister` (grave). I suoni che dipendono dal registro hanno due file, `_grave` e `_acuto`
(per esempio `s02_sfx_14_acuto.wav`), e Unity sceglie quello giusto.

### La sfida (Scena 3)

È la prima battaglia di [06-worked-example.md](music/06-worked-example.md):

1. la sigla di Sbuffo (`intro_sbuffo`), poi il conteggio TAC, TAC... TOC e «uno, due, tre... via!»;
2. alla fine del conteggio parte la base (8 battute, Re minore e Do5, 100 BPM) e **non si ferma
   fino alla vittoria**; sotto le voci si abbassa;
3. ogni turno è uno scambio lungo di 4 battute, allineato al cambio d'accordo: la frase due volte,
   il TOC sul quarto tempo («tocca a te»), l'eco del bambino, poi il pianoforte che la ripete;
4. il verdetto cade sul quarto tempo della battuta d'eco: un arpeggio Re-Fa-La-Do se va bene,
   una nota morbida se non ancora;
5. ogni turno riuscito aggiunge uno strato alla base (prima gli accordi, poi i timpani); un turno
   non riuscito toglie solo lo strato più alto, fa ridere Sbuffo (una battuta comica) e la frase
   torna nello scambio successivo, **mai più lenta**;
6. il terzo turno, come nei documenti, è la versione scombinata di Sbuffo, La... Fa-Re: entra l'anulare.
   Nessun turno chiede di premere nel silenzio di una nota mancante (era troppo difficile);
7. la base si ferma esattamente su una battuta e lì parte la fanfara, poi la ripresa con il Fa tornato.

Il giudizio dell'eco guarda i pulsanti nell'ordine giusto e il ritmo relativo (qualsiasi velocità
fra metà e il doppio, ogni intervallo entro 0,12 s o il 35%). La risposta del turno 3 vale da 0,35 s
prima a 0,45 s dopo il momento della nota. Dal pannello di debug (scheda Input) si vedono la modalità,
i tempi attesi, gli strati e il registro, e c'è «Pass it for me».

### Il suono libero (Scena 4)

Modalità `libero`: parte la canzone ritrovata con la campanella e un battito leggero (`x_s04_libero`),
ogni pulsante suona la sua nota di pianoforte nel registro scelto, e la storia prosegue quando la musica
finisce. Tutte le note dei pulsanti stanno bene sugli accordi (Fa, Re minore 7), quindi non c'è niente da giudicare.

### Scena 5 ed epilogo

Nella Scena 5 il tamburo (timpano su Re2, «Bum... Bum...») suona a sinistra e la campanella (celesta,
il suo motivo La-Do-La di 04-battle-cues.md) a destra, nella posizione in cui la lascia la Scena 4. Prima
della scelta si risentono uno dopo l'altro, con una pausa: il tamburo è il suono dell'indice, la campanella
del medio. La scelta non cambia il registro del pianoforte. Nel ramo A il tamburo si allontana, nel ramo B
suonano tre tintinnii.

Nell'epilogo l'interazione usa la modalità `libero` con un pezzo che è uno scambio completo
(`x_e00_eco`): una battuta di battito, la chiamata due volte con il TOC, la battuta del bambino, poi il
pianoforte la ripete comunque. Ogni pulsante suona la sua nota (tutte stanno bene su Re minore) e la storia
prosegue quando il pezzo finisce, anche se il bambino non ha premuto niente. Poi la ripresa lenta della
canzone (88 BPM: è una scena senza input, non una sfida), la risatina del Maestro e la canzone intera che sfuma.

## Musica e suoni

- Le partiture sono in [`music/library/capitolo_01.yaml`](../music/library/capitolo_01.yaml), nel formato
  di [05-format-and-pipeline.md](music/05-format-and-pipeline.md). Ogni partitura si controlla con
  `python3 -m tools.music.check music/library/capitolo_01.yaml` (nei tre registri), che rifiuta le note
  vietate, gli accordi non sicuri e la musica nel registro della chiamata.
- Differenze rispetto a 06, motivate nel file: la presa in giro dura una battuta (si suona nella battuta di risposta dello scambio non riuscito);
  con il registro grave basso e accordi sono rivoicati per lasciare libera la zona della chiamata.
- I suoni si rigenerano con
  `python3 tools/story_placeholder_sfx.py ~/.cache/echoquest-sfx/mono unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Sfx s02 s03 s04 s05 e00`.
  Gli strumenti sono sintetizzati in Python (`tools/music/synth.py`), lo stesso pianoforte del prologo:
  sono segnaposto finché non si installa FluidSynth con una SoundFont (05).
- La base è divisa in tre file (`x_s03_base_l0`, `_l1`, `_l3`) che suonano insieme sull'orologio audio;
  la chiamata del pianoforte sta circa 12 dB sopra la base.

## Verifiche

- `tools/story_interaction_checks.cs`: 80 controlli delle regole (eco, registro, Sì atteso, suono libero,
  nessuna modalità di risposta nel silenzio).
- `tools/story_battle_runtime_checks.cs`: in Play mode con i dati veri. `RunScene2` prova le quattro
  interazioni e la scelta del registro; `RunScene3` gioca la sfida premendo a tempo (turno 1 giusto,
  turno 2 prima sbagliato e poi giusto, turno 3 con l'anulare) e controlla base, strati e fanfara.
  `RunScene4` prova la canzone nel registro scelto, il suono libero e la domanda su Sbuffo.
  `RunScene5` prova la scelta tamburo/campanella (Sì non sceglie, il registro resta quello scelto).
  `RunEpilogue` prova lo scambio finale con tre note e senza nessuna risposta, e le varianti di registro.
- Restano verdi i controlli della Scena 1 (interazioni, percorsi, audio).

Da fare: ascolto in cuffia (livelli, timbri provvisori, attese fra un turno e l'altro), prova con il
controller fisico.
