# Story: capitolo 1, "La canzone rubata dal vento"

Materiale narrativo della demo: copione, voci registrate, liste di suoni e interazioni.
È una prima bozza, non ancora validata con bambini o esperti.

## Struttura

```
Story/
  Script/
    capitolo_01_la_canzone_rubata_dal_vento.md   copione originale (fonte di verità)
    capitolo_01_sequenza.csv      tutto il capitolo in ordine: voci, suoni, interazioni
    capitolo_01_voci.csv          una riga per battuta, testo pulito da dare al TTS
    capitolo_01_suoni.csv         una riga per [AUDIO]: suoni da trovare o produrre
    capitolo_01_interazioni.csv   [INTERAZIONE] e [CONVERGENZA]: cosa deve fare il gameplay
  Audio/
    Voice/                        voci registrate (ElevenLabs), un file per battuta
    Sfx/                          suoni, un file per [AUDIO], chiamato con l'ID del suono
```

I CSV si **generano** dal copione e non vanno modificati a mano. Dopo ogni modifica al copione,
o quando aggiungi una registrazione, rilancia dalla radice del repo:

```sh
S=unity/EchoQuestTest/Assets/EchoQuest/Story
python3 tools/story_extract.py $S/Script/capitolo_01_la_canzone_rubata_dal_vento.md \
  --out-dir $S/Script --name capitolo_01 --voice-dir $S/Audio/Voice --sfx-dir $S/Audio/Sfx
```

Lo script riconosce le sezioni scritte come `## Scena 1 — …` oppure `## 1. …`, e i rami scritti come
`### Ramo A — …` oppure `### Se il bambino sceglie …` (in questo caso le lettere A, B… vengono assegnate in ordine).

Attenzione: se nel copione aggiungi o togli una battuta, gli ID di quelle successive nella stessa
sezione e dello stesso personaggio scalano di uno. Le registrazioni già fatte vanno quindi rinominate.

## Provare la scena in Unity

1. Apri `Story/Scenes/Intro.unity` (doppio clic nella finestra Project) e premi **Play**.
   Meglio con le cuffie: il destra/sinistra si sente solo lì.
2. Parte il prologo (`p00`). Sullo schermo vedi dall'alto dove si trova ogni suono: il cerchio bianco
   sei tu, il naso indica davanti. Arancione = personaggi, marrone = voce lontana, azzurro = vento,
   giallo = suoni. Il Narratore non ha un pallino: si sente "dentro la testa".
3. Tasti: **Spazio** pausa · **R** ripeti la battuta · **N** salta · **Backspace** da capo ·
   **← →** gira la testa (i suoni si spostano intorno a te) · **Home** guarda di nuovo avanti ·
   **1–4** risposte alle scelte · **F9** registra quello che senti in un WAV (`unity/EchoQuestTest/Recordings/`, ignorata da git).

Per partire da un punto preciso: seleziona l'oggetto **Storia** nella scena e scrivi un ID in
*Start From* (per esempio `p00_maes_01`). Nello stesso pannello: *Section* (quale sezione suonare)
e *Take* (versione preferita delle voci, `a` o `b`).

### Regia: dove si trovano i suoni

`Script/capitolo_01_regia.csv` dice dove suona ogni personaggio e ogni suono, e si modifica a mano
(anche con un foglio di calcolo, salvando in CSV). Tu sei al centro e guardi avanti: `x` è la destra
(negativo = sinistra), `z` è davanti, `y` l'altezza, in metri circa.

- Una riga per personaggio (`narr`, `maes`, `lont`, `vent`) vale per tutte le sue battute; una riga con l'ID
  di una battuta o di un suono vale solo per quella.
- `x`/`y`/`z` vuoti = non spaziale (il Narratore). `a_x`/`a_y`/`a_z` = dove arriva, se si sposta.
- `volume` (0–1), `filtro_hz` (taglia gli acuti per sembrare più lontano o dietro una porta; vuoto = automatico
  in base alla distanza), `attesa` (secondi prima che parta la cosa successiva: serve a far parlare
  qualcuno sopra a una musica o a un vento).
- `ripeti` = `sì` su un suono: continua in loop, come sottofondo, finché non parte l'elemento scritto in `fino_a`.
- `sottofondo` su una battuta: l'ID di un suono che va in loop sotto quella battuta (per esempio la canzone
  spezzata mentre il Narratore dice «la canzone continua, ma non è più la stessa»).
- `pausa_dopo`: secondi di silenzio dopo quell'elemento.

Quando qualcuno parla, i sottofondi si abbassano da soli (a circa un terzo) e poi risalgono.
Le pause tra le battute sono automatiche: 0,6 s dopo ogni battuta, 1,2 s quando cambia chi parla.
Si regolano nell'oggetto **Storia** (*Pacing and music beds*); `pausa_dopo` vale per i casi singoli.

### Pause dentro le battute del Narratore

ElevenLabs legge un po' di fretta. `tools/stretch_pauses.py` allunga le pause tra le frasi dentro i file
(+0,35 s a ogni pausa più lunga di 0,28 s). Gli originali vanno in `Audio/Voice/_originali/` e i file in
`Audio/Voice/` vengono sempre rifatti da lì, quindi si può rilanciare quante volte si vuole:

```sh
V=unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Voice
python3 tools/stretch_pauses.py --voice-dir $V --originals-dir $V/_originali --chars narr   # --extra 0.5 per pause più lunghe
```

Dopo aver aggiunto nuove registrazioni del Narratore: rilancialo, poi rigenera la scena.

Più un suono è lontano, più è piano e "ovattato". Le cose nella sala hanno l'eco della sala, il Narratore no.
Limite attuale: Unity di base sposta i suoni solo tra destra e sinistra, quindi davanti e dietro non
si distinguono. Per un vero audio 3D in cuffia serve un plugin di spazializzazione (HRTF), da valutare.

La scena è generata da `Assets/EchoQuest/Editor/StorySceneBuilder.cs` (menu *EchoQuest → Rebuild Story Scene*):
dopo aver aggiunto file audio nuovi va rigenerata, così li trova.

## ID e nomi dei file

`p00_narr_01` = sezione + personaggio + numero (contato per sezione e personaggio).

| Sezione | Codice |  | Personaggio | Codice |
|---|---|---|---|---|
| Prologo | `p00` | | Narratore | `narr` |
| Scena 1–5 | `s01`…`s05` | | Maestro Battuta | `maes` |
| Epilogo | `e00` | | Vento (Sbuffo) | `vent` |
| | | | Voce lontana (il Maestro fuori scena) | `lont` |

Suoni: `p00_sfx_01`. Interazioni: `s01_int_01`.

File voce: `<id>_<versione>`, per esempio `p00_narr_01_a.mp3`.
- Più battute dello stesso personaggio in un solo file: `p00_narr_02_03_a.mp3`.
- Una battuta con N suoni **dentro** (colonna `dentro_battuta` di `capitolo_01_suoni.csv`) va divisa in
  N+1 pezzi, che Unity suona alternandoli ai suoni: `p00_narr_02_p1_a.wav`, suono, `p00_narr_02_p2_a.wav`, …

Esporta sempre in **mono**, perché l'audio spaziale di Unity funziona solo con clip mono.
Le registrazioni lunghe di ElevenLabs con più battute si dividono nelle pause; gli originali interi
restano fuori dal repo.

## Voci

| Personaggio | Voce ElevenLabs | Impostazioni |
|---|---|---|
| Narratore | Corrado Noventa – Mature, Deep and Rough | stability 50, similarity 75, modello v4 |
| Maestro Battuta | Antonio Farina – Expressive and Warm (provvisoria) | stability 50, similarity 75, modello v4 |
| Vento (Sbuffo) | da scegliere | |

La voce lontana è il Maestro Battuta fuori scena: stessa voce, resa "lontana" in Unity
(volume, riverbero, posizione).

## Stato delle registrazioni (prologo)

Il prologo ha 21 battute: **20 sono registrate**, manca solo `p00_vent_01` (il primo «Ffffff…» del Vento).
Ogni battuta ha due versioni, `_a` e `_b`: scegliete quella che preferite e cancellate l'altra.

Le voci per ora **non sono coerenti**, perché ogni registrazione ha usato una sola voce per tutti i personaggi:

| Battute | Voce usata | Da rifare |
|---|---|---|
| `p00_narr_01` … `p00_narr_03` | Corrado (Narratore) | no |
| `p00_lont_01/02`, `p00_maes_01` … `03` | Antonio | no |
| `p00_narr_04`, `p00_narr_05` | Antonio | sì, con la voce del Narratore |
| `p00_maes_04` … `08` | Corrado | sì, con la voce del Maestro |
| `p00_vent_02` … `04` | Corrado | sì, con la voce del Vento |
| `p00_narr_06`, `p00_narr_07` | Corrado | no |

## Suoni (prologo)

18 suoni su 19 ci sono, ma sono **provvisori** (`Audio/Sfx/`). Manca `p00_sfx_11` (risata in lontananza).

- Presi dal bundle Sonniss GDC 2026, parte 1 (licenza royalty-free, uso nei giochi consentito senza
  attribuzione; vietato rivenderli così come sono o usarli per addestrare AI): vento (`14`, `15`, `18`,
  `19`), porta e fogli (`15`), oggetto che cade (`09`), frugare (`10`), colpi di bacchetta (`12`),
  sedia che scricchiola (`01`), campanelli (`13`, `16`).
- **Sintetizzati** come segnaposto, perché nel bundle non ci sono: tutte le note di pianoforte
  (`01`–`06`, `13`, `16`, `17`), gli applausi (`07`, `08`), la cassa e il fischio del vento (`19`).
- Mancano ancora dentro i suoni composti: passi, porta che si apre e voci sottovoce (`01`), passi (`07`),
  qualcuno che si schiarisce la voce (`09`).

Il **tema musicale** del capitolo (pianoforte, `13`, `16`, `17`) è di tre note, Sol–Do–Mi, con il ritmo
«TA… TA-TA»: lo stesso che il bambino deve ripetere nella Scena 3. È un motivo nostro, non una canzone
esistente. Dopo che il vento lo ruba (`16`, `17`) diventa stonato e "storto": le prime due note calanti e
tremolanti, e al posto dell'ultima un tasto che si inceppa.

I suoni provvisori si rigenerano con `tools/story_placeholder_sfx.py` (istruzioni in testa al file).

Il copione completo (versione con la Scena 1 estesa) dura circa **30 minuti** di parlato (stima basata sul ritmo del Narratore,
circa 15 caratteri al secondo), più musica e suoni. Per il primo test conviene ridurlo
(vedi le note in fondo al copione).
