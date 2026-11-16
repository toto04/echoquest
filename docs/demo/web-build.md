# Automated web build

Last updated: 2026-11-16. Status: **planned, nothing implemented.**

Goal: every push to `main` produces a Web build of the demo at a stable link,
showing which commit it was built from.

Read [current-state.md](current-state.md) first.

## What is expected to break on the Web

Unity's Web player does not use its normal audio engine. It uses the browser's
Web Audio API and supports "only the basic features"
([Unity manual, Audio in Web](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/audio)).
The table compares that page with what the story player uses. Rows marked
"verify" are not stated by the manual and must be tested in a real build (task
W-02).

| Used by the project | Where | On the Web |
|---|---|---|
| `AudioSource.Play`, `PlayScheduled`, `loop`, `volume`, `timeSamples`, `isPlaying`, distance rolloff | `SpatialSource` | Supported |
| `AudioListener.pause` | `StoryPlayer.TogglePause` | Supported |
| Audio before any click or key press | `StoryPlayer.Start` starts after 1.5 s | **Blocked by every browser.** Needs a click-to-start screen |
| `AudioLowPassFilter` | `SpatialSource`: distance muffling and the `filtro_hz` column | Not listed; effects are unsupported. Expect **no muffling**. Verify |
| `AudioReverbZone`, `reverbZoneMix` | Concert-hall reverb built by `StorySceneBuilder` | Expect **no reverb**. Verify |
| `spatialBlend` 0 for the narrator, 1 for everything else | `SpatialSource.Init` | Not in the supported list. Verify that the narrator stays centred and that left/right panning works |
| `AudioSettings.dspTime` | Every `Wait()`, the rhythm clock, the missing-note schedule | Not mentioned. **If it does not advance, the story hangs.** Verify first |
| `OnAudioFilterRead`, `System.IO.File` | `ListenerRecorder` (F9) | Not supported. Compile it out |
| Preload Audio Data off | All 198 clips | Clips decode asynchronously in the browser. `WaitForPlayback` returns as soon as `isPlaying` is false, so **a clip that is not decoded yet may be skipped**. Verify, then preload or wait for `loadState` |
| Vorbis import | All clips | The Web player uses AAC. Re-encoded at build time; set the quality in a Web override |
| `timeSamples` against `clip.frequency` | `WaitForPlayback` overlap | The browser may resample; the manual says to check `AudioClip.frequency` at run time. Verify the negative `pausa_dopo` overlaps |
| Interface `HID` device matching | `EchoQuestController` | Never matches. See [Controller in the browser](#controller-in-the-browser) |
| `runInBackground` off | Project settings | The story freezes when the tab loses focus. Probably wanted; decide in W-06 |

Other things to check in the spike: the managed-code stripper removing the
custom Input System layout, the timing of `InputSystem.onEvent` against the
audio clock (rhythm tolerance is 0.10 s or 30%), and memory, since 25 minutes of
audio decoded to 32-bit float is about 265 MB.

## Build size

Estimate, to be replaced by the measured number from W-01: 25 minutes of mono
audio as AAC at about 96 kbit/s is roughly 18 MB, plus 10 to 15 MB for the
engine with URP. Around **30 to 35 MB** in total. At the current 100% quality
it could be twice that.

Limits that matter: GitHub Pages rejects files over 100 MB and sites over 1 GB.
Cloudflare Pages rejects files over 25 MiB, which the data file may exceed.

## Build script

One editor method, called by both the local command and CI, so they cannot
drift apart.

- `Assets/EchoQuest/Editor/DemoBuild.cs`, method
  `EchoQuest.Editor.DemoBuild.BuildWeb()`.
- Rebuilds the scenes from the builders first, so a build never ships a stale
  scene.
- Sets the scene list explicitly: the shell scene first (task S-01), then
  Intro, Scena1, ControllerTest.
- Writes the commit hash, branch and build time into a generated
  `BuildInfo` asset or class, read by the shell and the debug UI.
- Sets Web player options in code: compression, decompression fallback,
  template, product name.
- Reads the output folder from the `-buildOutput` argument that the Unity CLI
  forwards.

Local command, once the Web module is installed:

```sh
unity build unity/EchoQuestTest --target WebGL \
  --execute-method EchoQuest.Editor.DemoBuild.BuildWeb -o build/web
python3 -m http.server -d build/web 8000
```

Compression: GitHub Pages cannot send the `Content-Encoding` header that
Brotli and Gzip builds need. Use **Gzip with Decompression Fallback on**, or
compression off (Pages compresses some file types itself). itch.io serves
compressed Unity builds correctly.

## Continuous integration

Recommended: **GitHub Actions with GameCI** (`game-ci/unity-builder@v4`).

- The image `unityci/editor:ubuntu-6000.6.8f1-webgl-3` exists (checked on
  Docker Hub, 2026-11-16).
- Licence: a Unity Personal licence is activated with three repository secrets,
  `UNITY_LICENSE` (the contents of
  `/Library/Application Support/Unity/Unity_lic.ulf` on the Mac),
  `UNITY_EMAIL` and `UNITY_PASSWORD`
  ([GameCI activation guide](https://game.ci/docs/github/activation/)). Some
  third-party reports say Personal activation in CI is unreliable; W-11 proves
  it or falls back.
- Cache `unity/EchoQuestTest/Library` keyed on the hash of `Assets/`,
  `Packages/` and `ProjectSettings/`. Without it, every run re-imports 198
  audio clips.
- Trigger on pushes to `main` that touch `unity/**`, and by hand
  (`workflow_dispatch`). Firmware and docs commits should not rebuild.
- A private repository on the free plan has 2,000 Linux minutes a month. A
  cached build should take 10 to 20 minutes.

Fallback if the licence does not activate in the cloud: a **self-hosted runner
on the Mac**, or a `tools/deploy_web.sh` script that builds locally and pushes
the result. Both use the same `DemoBuild.BuildWeb`.

The existing story checks need a live Editor and are not part of CI in this
plan. Moving them to the Unity Test Framework so CI can run them is a possible
later task, not a requirement for the demo.

## Hosting

| Option | For | Against |
|---|---|---|
| **GitHub Pages** | Same place as the code, one stable URL, deploy from the workflow with `actions/deploy-pages` | Needs a paid plan for a private repository. The site is public to anyone with the link. No compression headers |
| **itch.io** with `butler push` | Free, handles Unity Web builds well, page can be restricted or password-protected, tutors know it | A second account and an API key secret. Less control over the page |
| Cloudflare Pages, Netlify | Free, private repository is fine | 25 MiB file limit on Cloudflare; another service to set up |

Recommendation: GitHub Pages if the account has Pro (the student pack includes
it), with one folder per build so older builds stay reachable:

```
/                 redirects to /latest/
/latest/          the newest build of main
/builds/<sha>/    kept for the last N builds
```

If the audio must not be public, use itch.io with a restricted page instead
(decisions 1 and 2 in [README.md](README.md#open-decisions)).

## Controller in the browser

Not needed for the first demo: keyboard and on-screen buttons cover everything.
Two routes exist for later.

1. **Browser Gamepad API.** The Input System shows the controller as a
   `WebGLGamepad` or `WebGLJoystick`, with buttons and axes as the browser
   chooses to expose them. Angles would arrive as generic axes, the hat mapping
   is browser-specific, and there is no output report, so no haptics. Whether
   Chrome exposes a Bluetooth LE HID gamepad this way on macOS and Windows has
   to be tried (W-15).
2. **WebHID** (Chrome and Edge only). A small `.jslib` plugin asks the user to
   pick the device, receives the raw 15-byte input reports, and queues them as
   state events on an `EchoQuestController` device added by hand. Everything
   downstream stays unchanged, including the raw-bytes readout, and output
   reports make haptics possible. More work, but it is the route that matches
   [`protocol/hid-report.md`](../../protocol/hid-report.md) exactly (W-16).

## Tasks

### Phase 1: spike

- [ ] **W-01 Build once by hand.** Human installs the Web module
  (`unity editors module --help` shows the command). Build with the default
  settings, serve locally, record the output size per file in this document.
  Done when the build loads in Chrome and Safari.
- [ ] **W-02 Audio feature test.** In that build, check each "verify" row of
  the table above and replace the row with the result. A throwaway test scene
  is fine. Done when every row says "works" or "does not work", with the
  browser tested.
- [ ] **W-03 Play both story scenes through** in the browser and list every
  difference from the Editor: skipped lines, wrong pauses, hangs, missing
  effects. Done when the list is in this document.
- [ ] **W-04 Input timing test.** Run the two rhythm exercises and the
  missing-note exercise ten times each in the browser. Done when the pass rate
  for correct taps is recorded, with the frame rate.

### Phase 3: web-ready runtime

- [ ] **W-05 Click-to-start.** Covered by the shell (S-02). Nothing plays
  before the first click or key press.
- [ ] **W-06 Platform guards.** Compile `ListenerRecorder` out on the Web;
  decide and set `runInBackground`; hide keys that do nothing in a browser.
- [ ] **W-07 Reverb and muffling.** Apply decision 4. Either accept the dry
  mix and label it in the build, or bake the effects into the clips offline.
- [ ] **W-08 Clip loading.** Fix whatever W-02 finds: turn preload on, or wait
  for `AudioClip.loadState` before playing. Consider giving each scene only its
  own section's clips to cut memory and load time.
- [ ] **W-09 Audio import override for the Web.** Set quality through an
  `AssetPostprocessor` or a preset, not by hand on 198 files. Long beds may use
  Compressed In Memory. Done when the build size is recorded and nothing sounds
  degraded in headphones.

### Phase 4: automation

- [ ] **W-10 `DemoBuild.BuildWeb`** as described above, with the local command
  documented in [../development-workflow.md](../development-workflow.md).
- [ ] **W-11 GitHub Actions workflow** `.github/workflows/web-build.yml` with
  licence secrets and Library cache. Human adds the secrets. Done when a push
  produces a build artifact.
- [ ] **W-12 Deploy step** to the chosen host. Human enables Pages or creates
  the itch.io project.
- [ ] **W-13 Build stamp.** The shell shows version, short commit hash and
  build date, so a tutor can say which build they saw.
- [ ] **W-14 Link and instructions** in the root `README.md`: the URL, what
  works in a browser and what does not, recommended browser, headphones.

### Phase 6: controller in the browser (optional)

- [ ] **W-15 Gamepad API test.** Pair the controller, open the build, record
  what the Input System reports. Done when the findings are written down, as
  was done for the Editor in [../unity-test-scene.md](../unity-test-scene.md).
- [ ] **W-16 WebHID plugin**, if W-15 shows the Gamepad API is not enough.
