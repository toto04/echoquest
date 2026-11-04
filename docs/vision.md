# Vision: what EchoQuest is

## In one sentence

EchoQuest is an **audio-only video game for blind and visually impaired
children**, built in Unity and played with a **custom Bluetooth controller**
that senses how it is held and moved and answers back with vibration.

## Who it is for

- **Players:** blind and visually impaired children. The game must be fully
  playable without seeing anything. Sound and touch carry all the information
  a sighted player would get from the screen.
- **People around them:** parents, teachers and therapists who set the game up,
  pair the controller and help the child get started. Setup must be possible
  without technical knowledge.
- **Developers:** a small team iterating quickly, largely with AI coding agents
  (see [development-workflow.md](development-workflow.md)).

## Why a custom controller

Off-the-shelf gamepads are built for sighted play. Their controls are many,
similar and hard to tell apart by touch, and they say little about how the
player is holding or moving them. EchoQuest's controller is meant to be:

- **Physical and spatial.** It reports its own **orientation** (yaw, pitch,
  roll) from an IMU, so turning, tilting and pointing the controller can be
  game actions. One natural use is turning toward a sound to face it, though
  the actual game mechanics are still to be designed (see
  [Open questions](#open-questions)).
- **Haptic.** The game can send **vibration commands** (which motor, how
  strong, how long, which pattern), so the controller can signal events,
  direction or proximity through touch, alongside the audio.
- **Simple and tactile.** A small number of distinct buttons that are easy to
  find and tell apart by touch.
- **Standard to the computer.** It appears to Windows and macOS as an ordinary
  **Bluetooth gamepad** (HID over GATT). No drivers, no companion app: pair it
  in the system Bluetooth settings and it works. The vibration channel is a
  vendor-defined extension to that standard device.

## What the game is (current understanding)

- Built in **Unity**, running on a computer (Windows/macOS) for now.
- **Audio-only:** spatial audio and sound design carry the world. Any visuals
  are optional and secondary, for the people helping, not for play.
- Played with the EchoQuest controller. Unity reads its buttons and
  orientation like any gamepad, and sends haptic commands back.

The game design itself (story, levels, mechanics) has **not been written
down yet**. This repository so far contains the controller and the protocol
between the controller and the game. That is deliberate: the controller's
capabilities shape what the game can do, so we build and prove the hardware
link first.

## Guiding principles (proposed)

These follow from the audience and are used to judge design choices. They
are proposals until the team confirms them.

1. **Nothing important is visual-only.** Every state the player needs
   (connected, paired, low battery, game events) must be conveyable through
   sound or vibration. The test board's LCD is a developer aid only.
2. **Zero-friction setup.** Standard Bluetooth pairing, no PIN, no drivers, and
   it reconnects by itself after the first pairing.
3. **Robust and forgiving.** Debounced buttons, haptics that stop if the
   connection drops, sensible behaviour when the controller is shaken, dropped
   or left on a table.
4. **Stable contract between controller and game.** One documented report
   layout ([`protocol/hid-report.md`](../protocol/hid-report.md)), versioned,
   with tests and a probe tool, so the firmware and the game can evolve
   independently.
5. **Fast iteration.** Cheap development boards, one-command builds, host-side
   tools that don't need Unity, and code organised so an AI agent can make
   changes safely.

## What "done" looks like for the first playable

Not yet fully defined. The minimal end-to-end target is:

1. A child holds the controller, it is already paired, and the game reacts to
   turning it and pressing its buttons.
2. The game makes the controller vibrate in response to events.
3. A non-technical adult can pair it the first time by following a short guide.

The Unity test scene ([unity-test-scene.md](unity-test-scene.md)) is the first
step toward point 1.

## Open questions

These are deliberately undecided. Record decisions in this file when they are made.

- **Game design:** genre, mechanics, story, session length, target age range.
- **How orientation is used:** absolute pointing, relative turning, gestures?
  This decides whether yaw drift matters and whether a magnetometer or a
  "recenter" button is needed (see [architecture.md](architecture.md#orientation)).
- **Controller form factor:** shape, size, button count and layout, and how
  buttons are made distinguishable by touch.
- **Haptics hardware:** motor type (ERM, LRA, other), how many, and where in the
  controller. The protocol already supports several motors and reserves pattern IDs.
- **Power:** battery type, charging, and how battery state is shown to a
  blind player.
- **Platforms:** desktop only, or also tablets/phones later?
- **Audio output:** headphones, speakers, or audio in the controller itself?
