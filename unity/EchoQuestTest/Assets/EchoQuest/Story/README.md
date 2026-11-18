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
    Interazioni/                  domande e risposte condizionali del Maestro; originali in _ricevuti/
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
2. Parte il prologo (`p00`). A sinistra c'è il **pannello di debug** (F1 lo nasconde): si usa col mouse
   e permette di saltare a qualsiasi battuta, cambiare sezione, premere i pulsanti del controller a
   schermo, regolare pause e volumi. Dettagli in `docs/demo/debug-ui.md`. Sullo schermo vedi dall'alto dove si trova ogni suono: il cerchio bianco
   sei tu, il naso indica davanti. Arancione = personaggi, marrone = voce lontana, azzurro = vento,
   giallo = suoni. Il Narratore non ha un pallino: si sente "dentro la testa".
3. Tasti: **Spazio** pausa · **R** ripeti la battuta · **N** salta durante il parlato · **Backspace** da capo ·
   **← →** gira la testa (i suoni si spostano intorno a te) · **Home** guarda di nuovo avanti ·
   **1–4** pulsanti musicali · **S oppure Y** Sì, **N** No nella Scena 1 · **F9** registra quello che senti in un WAV (`unity/EchoQuestTest/Recordings/`, ignorata da git).

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
  qualcuno sopra a una musica, a un vento o alla coda di un suono; il suono continua sotto la voce e si
  abbassa un po' mentre si parla).
- `ripeti` = `sì` su un suono: continua in loop, come sottofondo, finché non parte l'elemento scritto in `fino_a`.
- `sottofondo` su una battuta: l'ID di un suono che va in loop sotto quella battuta (per esempio la canzone
  spezzata mentre il Narratore dice «la canzone continua, ma non è più la stessa»).
- `pausa_dopo`: secondi di silenzio dopo quell'elemento. **Negativa** = la cosa successiva parte prima che
  questa finisca (per esempio `-0.3` su «Sono talmente serio che…»: la bacchetta cade sull'ultima parola).

Quando qualcuno parla, i sottofondi si abbassano da soli (a circa un terzo) e poi risalgono.
Le pause tra le battute sono automatiche: 0,6 s dopo ogni battuta, 1,2 s quando cambia chi parla.
Si regolano nell'oggetto **Storia** (*Pacing and music beds*); `pausa_dopo` vale per i casi singoli.

### Pause dentro le battute del Narratore

ElevenLabs legge un po' di fretta. `tools/stretch_pauses.py` allunga le pause tra le frasi dentro i file
(+0,35 s a ogni pausa più lunga di 0,28 s; +0,9 s dopo le domande con `--question-extra 0.9`). Il silenzio va
nella parte centrale della pausa, ad almeno 60 ms dalla voce, così non spezza le parole. Si inserisce silenzio pulito, senza ripetere
respiri o code di parole presenti nella pausa. Gli originali vanno in `Audio/Voice/_originali/` e i file in
`Audio/Voice/` vengono sempre rifatti da lì, quindi si può rilanciare quante volte si vuole:

```sh
V=unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Voice
python3 tools/stretch_pauses.py --voice-dir $V --originals-dir $V/_originali --chars narr   # --extra 0.5 per pause più lunghe
```

Dopo aver aggiunto nuove registrazioni del Narratore: rilancialo, poi rigenera la scena.
Con `--only 's01_maes_1*'` rifà solo i file che corrispondono. Dopo una domanda allunga solo una pausa
vera: se la voce prosegue senza pausa non inserisce niente, perché il punto più silenzioso vicino a una
parola è spesso il silenzio prima di una "p" o di una "t", e il silenzio messo lì spezza la parola.

### Accorciare una battuta già registrata

Se nel copione si **tolgono** frasi da una battuta (senza aggiungere parole), non serve registrarla di nuovo:
`tools/voice_trim.py` trova le parole tolte nell'audio e le taglia nelle pause intorno.

```sh
python3 tools/voice_trim.py vecchio.wav nuovo.wav --old-text "testo vecchio" --new-text "testo nuovo" \
    --whisper-model ~/.local/share/whisper-models/ggml-large-v3-turbo.bin
```

Per Narratore e Maestro si accorcia il file in `Audio/Voice/_originali/` e poi si rilancia `stretch_pauses.py`.
Se togliere battute o `[AUDIO]` fa scalare i numeri, i file vanno rinominati (vedi sotto).

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

Ci sono tutti i 23 suoni, ma sono **provvisori** (`Audio/Sfx/`).
I numeri seguono `capitolo_01_suoni.csv`: se nel copione si aggiunge un `[AUDIO]`, quelli dopo scalano
e i file vanno rinominati (è successo aggiungendo i suoni `09`–`12`).

Nella sala ci sono quattro strumenti, sempre negli stessi posti (vedi la regia): **pianoforte** davanti a
destra, **tamburo** a sinistra, **triangolo** a destra in alto, **flauto** davanti. Quando il Narratore nomina
un suono (lungo, corto, delicato, forte, «bum bum», «din din», «suoni delicati», «tutti insieme») lo si
sente davvero, con uno strumento diverso ogni volta (`03`–`06`, `09`–`12`).

- Presi dal bundle Sonniss GDC 2026, parti 1, 2 e 5 (licenza royalty-free, uso nei giochi consentito senza
  attribuzione; vietato rivenderli così come sono o usarli per addestrare AI): vento (`18`, `19`, `22`,
  `23`), porta e fogli (`19`), oggetto che cade (`13`), frugare (`14`), colpi di bacchetta (`16`),
  sedia che scricchiola (`01`), risata lontana (`15`).
- **Sintetizzati** come segnaposto: pianoforte, tamburo, triangolo, flauto, applausi (`07`, `08`) e il
  fischio del vento (`23`).
- Mancano ancora dentro i suoni composti: passi, porta che si apre e voci sottovoce (`01`), passi (`07`),
  qualcuno che si schiarisce la voce (`13`).

Il **tema musicale** del capitolo è di tre note, Sol–Do–Mi, con il ritmo «TA… TA-TA»: lo stesso che il
bambino deve ripetere nella Scena 3. È un motivo nostro, non una canzone esistente.
- `17` è la canzone dell'orchestra e **si costruisce sotto il Narratore**: prima solo il pianoforte, poi
  entra il tamburo («poi ne arriva un altro»), il triangolo («e un altro ancora»), il flauto e il basso
  («ogni strumento…»), e cresce davvero di volume (circa +10 dB) fino a «la musica continua a crescere».
  Poi continua piena, in loop, finché il vento non la rovina (`20`). I tempi sono allineati alla
  battuta `p00_narr_05`: se la si rigenera, vanno ricontrollati in `tools/story_placeholder_sfx.py`.
- Dopo che il vento la ruba (`20`, `21`) diventa stonata e "storta": le prime due note calanti e tremolanti,
  e al posto dell'ultima un tasto che si inceppa.

I suoni provvisori si rigenerano con `tools/story_placeholder_sfx.py` (istruzioni in testa al file).
I file sorgente (Sonniss e Kenney convertiti in mono) non stanno nel repo, perché la licenza Sonniss vieta di
ridistribuirli così come sono. Sul computer di Bianca sono in `~/.cache/echoquest-sfx/mono`:

```sh
python3 tools/story_placeholder_sfx.py ~/.cache/echoquest-sfx/mono unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Sfx s01
```

## Scena 1 (`s01`, scena `Story/Scenes/Scena1.unity`)

Versione interattiva: **93 elementi, 45 battute, 40 suoni e otto interazioni**,
compresa quella nel ramo facoltativo di riascolto.

- Tocco viene presentato subito dopo il Maestro. Si prova l’indice, poi si
  distinguono una volta sola i quattro pulsanti musicali dai due del pollice.
- Le risposte usano Sì/No: sulla tastiera **S o Y** per Sì, **N** per No. Le domande successive non
  ripetono quale pulsante premere. **1** serve per i tre esercizi musicali.
- Il bambino ripete un ritmo regolare e TAC TAC… TOC, confronta i due ritmi,
  preme al momento della nota mancante e risponde alla richiesta d’aiuto.
- No alla richiesta d’aiuto apre un’attesa calma. Si continua con Sì, senza
  ripetere la domanda o far partire automaticamente la storia.
- La scelta finale conserva i due percorsi: Y va al pianoforte; N riascolta.
  Dopo il riascolto si può confermare con Y oppure aspettare con N.
- Le risposte sbagliate negli esercizi fanno riascoltare l’esempio senza
  rileggere la consegna. R la ripete se richiesta.
- La descrizione di Sbuffo resta, senza ripetere il furto. Restano «Un panino?!»
  e il mistero sul vento; è tolta la gag che ritardava la presentazione di Tocco.
- Registrazioni complete: non sono richiesti nuovi audio. Gli MP3 originali
  restano in `Audio/Interazioni/_ricevuti/`, esclusi dalle scene.
- I GUID dei file riutilizzati sono conservati. La regia e il generatore dei
  suoni seguono i nuovi numeri; gli esempi degli esercizi hanno nomi `x_s01_*` stabili.

La musica rubata resta distinta dagli esempi: il ricordo contiene le tre note
Sol–Do–Mi, mentre il pianoforte attuale perde la terza. L’esercizio sincronizzato
usa esattamente i tempi del ricordo. I passi, i colpi e i suoni provvisori usano
le fonti Kenney CC0 e Sonniss già documentate; gli esempi di strumenti restano
sintetizzati. Nessuna nuova dipendenza o modifica del protocollo.

Il [piano e le verifiche](../../../../../docs/scena1-interattiva.md) riportano
punti di prova, configurazione e limiti. Sono superati i test delle regole,
quelli dell’input in Play e i due percorsi della scena. Il riascolto in cuffia
resta necessario per valutare raccordi e riverbero.

## Scene 2 e 3 (`s02`, `s03`, scene `Scena2.unity` e `Scena3.unity`)

Il pianoforte e la prima sfida contro Sbuffo, giocabili con voci, suoni e musica. Le scene del capitolo
si caricano una dopo l'altra (Intro → Scena 1 → Scena 2 → Scena 3 → Scena 4). Nella Scena 2 il bambino sceglie
il registro del pianoforte (indice = grave, medio = acuto): i suoni con i file `_grave`/`_acuto` seguono
la scelta. La Scena 3 è una battaglia a tempo su una base che non si ferma, descritta in
[scena2-3-interattive.md](../../../../../docs/scena2-3-interattive.md) insieme alle verifiche.

La musica è scritta come partiture a griglia in `music/library/capitolo_01.yaml` (controllate da
`python3 -m tools.music.check`) e i suoni si rigenerano come quelli del prologo:

```sh
python3 tools/story_placeholder_sfx.py ~/.cache/echoquest-sfx/mono unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Sfx s02 s03
```

Nella regia, le nuove colonne: `risposta` (la risposta giusta di un confronto, `si` o `no`),
`registro` (per una scelta: `1=grave;2=acuto`), `pulsanti` (i pulsanti della frase di una sfida),
`base` e `battute` (su un suono: la base della sfida che parte quando il suono finisce; `fino_a`
dice dove si ferma), `primo_ascolto` (`storia`: gli esempi li ha appena suonati la storia). Modalità nuove:
`sfida` (eco sulla base) e `libero` (Scena 4: il bambino suona insieme alla canzone, senza note sbagliate).
Nessuna interazione chiede di premere a tempo nel silenzio di una nota mancante: era troppo difficile.

La Scena 4 (`Scena4.unity`) ha voci, suoni, regia e le sue due interazioni; i suoni si rigenerano con `s04`.

