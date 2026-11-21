# A Serious-Game Paradigm to Evaluate Human Perceptual and Behavioural Responses to Drone Acoustics

* **Author:** Samarth Wal (`21AE30022`)
* **Institution:** Department of Aerospace Engineering, Indian Institute of Technology Kharagpur
* **Supervisors:** Prof. Sandeep Saha (IIT Kharagpur), Prof. Cade McCall (University of York)
* **Thesis Submission:** May 2026 (Project-II, `AE57004`)
* **Engine Version:** Unity `6000.0.55f1`

---

## Overview

This repository contains the Unity source project for a 3D first-person foraging serious game developed to investigate how drone acoustics modulate human perceptual and behavioural responses under simulated threat[cite: 6, 18].

Participants forage for clustered resources (**Red Apples = 10 pts**, **Golden Apples = 50 pts**) across a **120-second trial** in a fantasy forest environment while being tracked by an autonomous aerial drone hovering at a **10 m altitude**[cite: 18, 20]. To isolate the effect of drone acoustics on risk-taking and explore-exploit trade-offs, two conditions are compared while keeping visual drone behaviour identical[cite: 16, 21]:

* **High-Noise Condition:** Full-volume continuous propeller audio loop[cite: 21].
* **Low-Noise Condition:** Identical propeller audio passed through a **5000 Hz low-pass filter**[cite: 21].

When the drone locks on and approaches, players incur proximity score penalties unless they adopt defensive hiding behaviour (**crouching**)[cite: 6].

---

## Demo Video

Because the full-resolution gameplay demonstration video is **142 MB**, it is hosted via **GitHub Releases** rather than tracked directly in the Git commit history:

* **Watch / Download Demo Video:** [View on the Releases Page](../../releases)

---

## Key Experimental Findings ($N = 31$)

* **Cognitive Risk Recalibration:** Under the High-Noise condition, participants initiated crouching earlier ($p = 0.0521$)[cite: 26, 28], waited until the drone retreated significantly further before standing back up ($p = 0.0448$)[cite: 26, 28], and maintained a larger safety distance from the drone when collecting apples ($p = 0.0143$)[cite: 26, 29].
* **Motor & Spatial Stability:** Elevated drone noise did not impair physical navigation, transit times ($p = 0.4269$), obstacle collisions ($p = 0.7790$), or core foraging routes[cite: 26, 30, 31].
* **Temporal Habituation:** Defensive crouch-initiation distances decreased during the second half of the 120-second trial, indicating gradual habituation to the continuous acoustic stressor[cite: 30, 31].

---

## Repository Structure

```text
├── Assets/            # Custom scripts, scenes, prefabs, audio filters, and terrain assets
├── Packages/          # Unity Package Manager manifest and dependency lockfile
├── ProjectSettings/   # Input, physics, audio, tag, and quality configurations
└── .gitignore         # Excludes auto-generated Library, Temp, Logs, and Builds folders
