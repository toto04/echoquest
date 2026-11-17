# Scene 2 e 3 interattive

Versione del 17 novembre 2026. Le due scene si giocano in Unity
(`Story/Scenes/Scena2.unity` e `Scena3.unity`), con tutte le voci, i suoni e la musica.
Le scene del capitolo si caricano una dopo l'altra: Intro → Scena 1 → Scena 2 → Scena 3.
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
| 3 | `s03_int_04` | sfida, turno 3: Do, La... e il Fa | il medio nel momento della terza nota |

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
6. l'ultimo turno è una risposta: il pianoforte suona Do, La... e il bambino mette il Fa rubato;
7. la base si ferma esattamente su una battuta e lì parte la fanfara, poi la ripresa con il Fa tornato.

Il giudizio dell'eco guarda i pulsanti nell'ordine giusto e il ritmo relativo (qualsiasi velocità
fra metà e il doppio, ogni intervallo entro 0,12 s o il 35%). La risposta del turno 3 vale da 0,35 s
prima a 0,45 s dopo il momento della nota. Dal pannello di debug (scheda Input) si vedono la modalità,
i tempi attesi, gli strati e il registro, e c'è «Pass it for me».

## Musica e suoni

- Le partiture sono in [`music/library/capitolo_01.yaml`](../music/library/capitolo_01.yaml), nel formato
  di [05-format-and-pipeline.md](music/05-format-and-pipeline.md). Ogni partitura si controlla con
  `python3 -m tools.music.check music/library/capitolo_01.yaml` (nei tre registri), che rifiuta le note
  vietate, gli accordi non sicuri e la musica nel registro della chiamata.
- Differenze rispetto a 06, motivate nel file: il terzo turno è una risposta invece di un'altra eco;
  la presa in giro dura una battuta (si suona nella battuta di risposta dello scambio non riuscito);
  con il registro grave basso e accordi sono rivoicati per lasciare libera la zona della chiamata.
- I suoni si rigenerano con
  `python3 tools/story_placeholder_sfx.py ~/.cache/echoquest-sfx/mono unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Sfx s02 s03`.
  Gli strumenti sono sintetizzati in Python (`tools/music/synth.py`), lo stesso pianoforte del prologo:
  sono segnaposto finché non si installa FluidSynth con una SoundFont (05).
- La base è divisa in tre file (`x_s03_base_l0`, `_l1`, `_l3`) che suonano insieme sull'orologio audio;
  la chiamata del pianoforte sta circa 12 dB sopra la base.

## Verifiche

- `tools/story_interaction_checks.cs`: 80 controlli delle regole (anche eco, risposta, registro, Sì atteso).
- `tools/story_battle_runtime_checks.cs`: in Play mode con i dati veri. `RunScene2` prova le quattro
  interazioni e la scelta del registro; `RunScene3` gioca la sfida premendo a tempo (turno 1 giusto,
  turno 2 prima sbagliato e poi giusto, turno 3 risposta) e controlla base, strati e fanfara.
- Restano verdi i controlli della Scena 1 (interazioni, percorsi, audio).

Da fare: ascolto in cuffia (livelli, timbri provvisori, attese fra un turno e l'altro), prova con il
controller fisico, voci e suoni delle Scene 4, 5 e dell'epilogo.
