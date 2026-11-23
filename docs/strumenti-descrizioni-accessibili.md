# Raccontare gli strumenti a bambini ciechi

Il gioco deve funzionare anche per chi non ha mai visto. Le descrizioni non richiedono
ricordi visivi: partono da suono, materiali, parti, spazio e movimenti delle mani.
Non si chiede di chiudere gli occhi, riconoscere colori o guardare un oggetto.

Le mani restano sul controller: «immagina di seguire il bordo» è una descrizione,
non la richiesta di toccare uno strumento che il gioco non mette a disposizione.
Un termine come «rotondo» viene accompagnato dal percorso: seguire il bordo,
fare tutto il giro e tornare al punto di partenza. «Vibrare» viene spiegato come
«tremare molto in fretta». Dopo la spiegazione arriva il suono dello strumento.

## Aggiornamento del 23 novembre 2026

- **Pianoforte, Scena 2:** la storia di Cristofori è già presente. La presentazione
  spiega cassa di legno, gambe, corde e fila di leve lisce; alcuni tasti sono rialzati.
  Si conserva il rumore del tasto fra le due parti della registrazione.
- **Tamburo, Prologo:** breve racconto dei tamburi di legno e pelle nell'antico Egitto,
  portati anche appesi al collo. Non si attribuisce a qualcuno l'invenzione del tamburo.
- **Tamburo, Scena 1:** si distingue la famiglia dei tamburi dal timpano che si sente
  nel gioco: recipiente di metallo cavo, bordo rotondo, pelle tesa e battente morbido.
  La similitudine della fila che cammina «senza guardarsi» è sostituita da un gruppo
  che comincia a cantare in momenti diversi.
- **Campanella:** breve richiamo alle campane dell'antica Roma; manico, corpo cavo,
  apertura rivolta in basso e batacchio che colpisce la parete dall'interno.

Gli ID restano gli stessi: sei registrazioni del Maestro cambiano, senza rinumerare
le battute, gli audio di scena o le interazioni. Voce Antonio Farina, modello e
impostazioni delle registrazioni precedenti; file mono e pause elaborate dagli
originali con `tools/stretch_pauses.py`.

## Verifica delle registrazioni

Le sei battute sono registrate con Antonio Farina (`eleven_v4`, stabilità 0,5,
similarità 0,75), convertite in WAV mono a 44.100 Hz. Gli originali sono conservati
in `Audio/Voice/_originali/`; gli stessi GUID degli audio riprodotti restano validi.
Le pause interne vengono allungate di 0,35 secondi quando durano almeno 0,28 secondi.

Sette controlli in Play mode con Unity 6000.6.8f1 verificano caricamento e fine delle
sei battute, più il suono del tasto fra le due parti della spiegazione del pianoforte.
I livelli di picco restano sotto 0 dBFS; il volume medio è confrontabile con le battute
vicine. La trascrizione automatica delle sei battute è stata controllata; due passaggi
poco chiari nella prima trascrizione sono stati confermati con un secondo modello.
ID e ordine degli elementi nei tre capitoli sono invariati.

## Fonti storiche

- [The Met, tamburo egizio, 664–332 a.C.](https://www.metmuseum.org/art/collection/search/552813):
  legno e pelle; strumenti di questo tipo appesi al collo e suonati con le mani.
- [The Met, campanella romana del I secolo d.C.](https://www.metmuseum.org/art/collection/search/502178):
  bronzo, uso di campane nella vita romana e possibili applicazioni agli animali.
- [The Met, timpano con corpo in rame e pelle](https://www.metmuseum.org/art/collection/search/501905).
- [The Met, collezione di strumenti musicali](https://www.metmuseum.org/departments/musical-instruments):
  pianoforte di Cristofori del 1720, costruito a Firenze.

L'ascolto con bambini ciechi e il riscontro di educatori restano necessari per valutare
comprensione, durata e parole nuove; questi testi non sono ancora validati con loro.
