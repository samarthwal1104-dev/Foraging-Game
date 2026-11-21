# A Serious-Game Paradigm to Evaluate Human Perceptual and Behavioural Responses to Drone Acoustics

* **Author:** Samarth Wal (`21AE30022`)
* **Institution:** Department of Aerospace Engineering, Indian Institute of Technology Kharagpur
* **Supervisors:** Prof. Sandeep Saha (IIT Kharagpur), Prof. Cade McCall (University of York)
* **Thesis Submission:** May 2026 (Project-II, `AE57004`)
* **Engine Version:** Unity `6000.0.55f1`

---

## Overview

This repository contains the Unity source project for a 3D first-person foraging serious game developed to investigate how drone acoustics modulate human perceptual and behavioural responses under simulated threat.

Participants forage for clustered resources (**Red Apples = 10 pts**, **Golden Apples = 50 pts**) across a **120-second trial** in a fantasy forest environment while being tracked by an autonomous aerial drone hovering at a **10 m altitude**. To isolate the effect of drone acoustics on risk-taking and explore-exploit trade-offs, two conditions are compared while keeping visual drone behaviour identical:

* **High-Noise Condition:** Full-volume continuous propeller audio loop.
* **Low-Noise Condition:** Identical propeller audio passed through a **5000 Hz low-pass filter**.

When the drone locks on and approaches, players incur proximity score penalties unless they adopt defensive hiding behaviour (**crouching**).

---

## Demo Video

Because the full-resolution gameplay demonstration video is **142 MB**, it is hosted via **GitHub Releases** rather than tracked directly in the Git commit history:

* **Watch / Download Demo Video:** [View on the Releases Page](../../releases)

---

## Key Experimental Findings ($N = 31$)

* **Cognitive Risk Recalibration:** Under the High-Noise condition, participants initiated crouching earlier ($p = 0.0521$), waited until the drone retreated significantly further before standing back up ($p = 0.0448$), and maintained a larger safety distance from the drone when collecting apples ($p = 0.0143$).
* **Motor & Spatial Stability:** Elevated drone noise did not impair physical navigation, transit times ($p = 0.4269$), obstacle collisions ($p = 0.7790$), or core foraging routes.
* **Temporal Habituation:** Defensive crouch-initiation distances decreased during the second half of the 120-second trial, indicating gradual habituation to the continuous acoustic stressor.

---

## Repository Structure

```text
├── Assets/            # Custom scripts, scenes, prefabs, audio filters, and terrain assets
├── Packages/          # Unity Package Manager manifest and dependency lockfile
├── ProjectSettings/   # Input, physics, audio, tag, and quality configurations
└── .gitignore         # Excludes auto-generated Library, Temp, Logs, and Builds folders
```

---

## Setup & Reproduction

1. Install **Unity Hub** and download Unity Editor **`6000.0.55f1`**.
2. Clone this repository:
   ```bash
   git clone [https://github.com/YOUR_USERNAME/Foraging-Game.git](https://github.com/YOUR_USERNAME/Foraging-Game.git)
   ```
3. In Unity Hub, click **Add → Add project from disk** and select the cloned folder (Unity will automatically regenerate `Library/` on first launch).
4. Open the main scene inside `Assets/` and press **Play** (`WASD` to move, mouse to look, `Ctrl` / `C` to crouch).

---

## Output & Data Logging

Each 120-second trial automatically exports two files for behavioural analysis:
* **`TrialData.csv`:** Summary performance metrics and timestamped event logs (`AppleCollected`, `CrouchEvent`, `ObstacleCollision`, Inter-Target Time, Inter-Target Distance, Area-Restricted Search index, and drone proximity).
* **`MovementMap.png`:** Top-down 2D trajectory plot contrasting player (blue) and drone (red) paths.

---

## Citation

If you use this experimental paradigm or codebase in your research, please cite:

```bibtex
@mastersthesis{wal2026droneforaging,
  author  = {Wal, Samarth},
  title   = {A Serious-Game Paradigm to Evaluate Human Perceptual and Behavioural Responses to Drone Acoustics},
  school  = {Indian Institute of Technology Kharagpur},
  year    = {2026},
  month   = {May},
  note    = {Supervised by Prof. Sandeep Saha and Prof. Cade McCall}
}
```
