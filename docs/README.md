# EchoQuest documentation

Start here if you are new to the project, human or AI.

| Document | What it covers |
|---|---|
| [vision.md](vision.md) | The idea: who EchoQuest is for, what the game and controller are meant to be, guiding principles, and what is still undecided |
| [architecture.md](architecture.md) | How the pieces fit together: controller hardware, firmware, BLE HID protocol, host tools, Unity. Includes the design decisions and why we made them |
| [roadmap.md](roadmap.md) | Where the project is now, what's been verified, and what comes next |
| [unity-test-scene.md](unity-test-scene.md) | The next milestone: a small Unity scene to try the controller end to end, and how it will be built |
| [scena1-interattiva.md](scena1-interattiva.md) | Five approved Scene 1 changes, reuse of existing recordings, new voice lines and gameplay verification |
| [demo/](demo/README.md) | Plan for the shareable demo: current state of the Unity project, automated web build, debug UI and demo shell, with task lists |
| [development-workflow.md](development-workflow.md) | How we work: AI-assisted development with Claude Code, PlatformIO and the Unity CLI, and the rules for changes |
| [music/](music/README.md) | Music composition guidelines for battles: the fixed four-note palette, writing calls and backing loops, the battle cue sheet, the snippet format and checks for AI agents, a worked example, and open questions |

Reference material that lives elsewhere in the repo:

- [`../README.md`](../README.md): supported boards, flashing, manual hardware test steps, troubleshooting.
- [`../protocol/hid-report.md`](../protocol/hid-report.md): **single source of truth** for the HID report byte layout.
- [`../CLAUDE.md`](../CLAUDE.md): short operating instructions for AI coding agents (layout, commands, conventions).
