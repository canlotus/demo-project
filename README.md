# Prototype Submission (Unity) — Card Match Game

This repository contains a functional prototype of a 2D card-match (memory) game developed from scratch in **Unity 2021 LTS**.

## Features
- **Dynamic board layouts** (e.g., 2x2, 3x3, 5x6) driven by ScriptableObject configuration  
- **Scales to fit** the target UI container using `GridLayoutGroup` auto cell sizing
- **Card flip animation** + match/mismatch feedback (flash + flip back)
- **Continuous flipping**: player can keep selecting cards while previous comparisons are resolving  
  - Max **4** unresolved face-up cards (2 pairs) to keep gameplay clear
  - Each selected **pair is evaluated within itself**
- **Save/Load** persistence between restarts (progress, attempts, matches, matched cells, preview state, seed)
- **Game over flow**: completion panel shown when all pairs are matched; **Continue** becomes unavailable after completion
- **SFX**: flip, match, mismatch, and game over (win)
- **Button click SFX** via `ButtonSfx` component

## How to Run
1. Open the project with **Unity 2021 LTS**.
2. Open `MenuScene` and press **Play**.
3. Choose difficulty and start a new game, or continue if available.

## Scenes
- `MenuScene`: Difficulty selection + New Game / Continue
- `GameScene`: Gameplay board + HUD + Game Over panel

## Controls
- **Mouse / Touch**: Tap cards to flip.

## Saving
Progress is stored as JSON in:
- `Application.persistentDataPath/save.json`

To reset progress:
- Start a **New Game** (overwrites save), or delete the save file manually.

## Project Structure (high level)
- `Scripts/`
  - `GameController` (gameplay flow, input, resolving pairs, save updates)
  - `BoardBuilder` (builds board grid from layout + sprites)
  - `CardView` (flip animation, mismatch flash, vanish to empty)
  - `SaveSystem` / `SaveData` (persistence)
  - `AudioManager` + `ButtonSfx` (SFX)
- `ScriptableObjects/`
  - Difficulty database (board sizes, empty cells, preview seconds, sprites)

## Notes
- The repository name can be changed to a random/obscure name to avoid revealing the game type (per assignment request).

Thank you for reviewing my submission.
