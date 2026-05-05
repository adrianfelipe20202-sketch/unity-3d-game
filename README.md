# :joystick: Unity 3D Adventure Game

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-000000?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-11-239120?logo=csharp&logoColor=white)
![URP](https://img.shields.io/badge/Render%20Pipeline-URP-blueviolet)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-Playable-brightgreen)

A **first-person 3D adventure game** built entirely in Unity with C#. Explore three hand-crafted levels, dodge AI-driven enemies, collect golden orbs, find a hidden gem, and unlock chests to advance through increasingly challenging stages -- all with a custom HUD, stamina system, and procedural particle effects.

---

## :sparkles: Features & Game Mechanics

### Player Controller
- **WASD movement** with smooth `CharacterController`-based physics
- **Sprint** (Left Shift) with a full stamina system -- drain, recharge, and lockout threshold
- **Double-checked jump** using both `isGrounded` and a `SphereCast` for reliable ground detection
- **Mouse look** with clamped vertical rotation and cursor lock
- **Health & damage** system with on-screen HUD bars

### Enemies (AI)
- Distance-based **chase AI** that pursues the player on the XZ plane
- **Proximity melee attack** with configurable damage and 1.2-second cooldown
- Enemies scale in speed and count per level for increasing difficulty

### Collectibles & Progression
- **Golden Orbs** -- floating, rotating spheres with particle burst on pickup
- **Special Gem** (cyan/violet) -- a unique item required alongside orbs to unlock chests
- **One-at-a-time spawner** -- only one orb exists at a time; a new one spawns after collection
- **Platform spawner** (Level 3) -- items spawn on top of moving platforms

### Interactive Objects
- **NPC Wizard** -- dialogue system triggered with `E` key (cycles through multiple lines)
- **Chest** -- requires all orbs + gem to open; triggers golden particle burst and victory state
- **Door** -- toggles open/close; when the chest is unlocked, loads the next scene automatically
- All interactables inherit from a shared `ObjetoInteractivo` base class

### Level Design
| Level | Theme | Enemies | Key Challenge |
|-------|-------|---------|---------------|
| **1 -- The Fields** | Blue procedural sky, green floor | 1 enemy | Learn the basics: collect 5 orbs + gem, open chest |
| **2 -- The Dark Arena** | Dark violet atmosphere, rock floor | 2 faster enemies | 8 floating platforms, chest elevated at Y=10.5 |
| **3 -- Moving Platforms** | Fog, dim lighting, dark floor | -- | PingPong & linear moving platforms, items spawn on platforms |

### HUD & UI
- Real-time **health bar** and **stamina bar** driven by `anchorMax` scaling
- **Item counter** with dynamic objective text
- **Temporary message** system for gameplay feedback
- **Victory overlay** on level completion
- **Defeat screen** -- red "DEFEATED!" overlay on death with `R` to retry

---

## :hammer_and_wrench: Tech Stack

| Technology | Purpose |
|------------|---------|
| **Unity 2022.3 LTS** | Game engine |
| **C# (.NET)** | All gameplay scripts |
| **URP (Universal Render Pipeline)** | Rendering |
| **Unity UI (Canvas)** | HUD, menus, overlays |
| **CharacterController** | Player physics |
| **ParticleSystem** | Procedural VFX on pickup and chest open |
| **Editor Scripting** | Automated level generation via custom menu items |

---

## :file_folder: Project Structure

```
Assets/
├── Editor/
│   ├── ConfiguradorSemana9.cs       # Menu tool: auto-configure Level 1
│   ├── ConfiguradorNivel2.cs        # Menu tool: generate Level 2 scene
│   └── ConfiguradorNivel3.cs        # Menu tool: generate Level 3 scene
│
├── Scripts/
│   ├── Jugador.cs                   # Player controller (movement, sprint, jump, health)
│   ├── Enemigo.cs                   # Enemy AI (chase + melee attack)
│   ├── NPC.cs                       # Dialogue NPC (inherits ObjetoInteractivo)
│   ├── ObjetoInteractivo.cs         # Base class for E-key interactables
│   ├── Cofre.cs                     # Chest: requires orbs + gem to unlock
│   ├── Puerta.cs                    # Door: open/close, scene transition
│   ├── Coleccionable.cs             # Golden orb: float, rotate, collect
│   ├── ItemEspecial.cs              # Special gem collectible
│   ├── GeneradorItems.cs            # Sequential spawner (one at a time)
│   ├── SpawnerItemsEnPlataformas.cs # Spawns items on moving platforms
│   ├── MovimientoPingPongLineal.cs  # Platform movement (linear & ping-pong)
│   ├── ContadorItems.cs             # HUD: item counter, objectives, messages
│   ├── GestorJuego.cs               # Static game state manager
│   ├── HUDJugador.cs                # Health & stamina bar updater
│   ├── PantallaDerrota.cs           # Defeat screen overlay
│   └── DiagnosticoEscenas.cs        # Scene diagnostics utility
│
├── Scenes/
│   ├── SampleScene.unity            # Level 1
│   ├── Nivel2.unity                 # Level 2
│   └── Nivel3.unity                 # Level 3
│
├── Materials/                       # URP Lit materials (procedural)
└── Prefabs/                         # Reusable prefabs (collectibles, etc.)
```

---

## :rocket: Getting Started

### Prerequisites
- **Unity 2022.3.62f3** (LTS) or compatible 2022.3.x version
- Unity Hub installed

### How to Run
1. **Clone the repository**
   ```bash
   git clone https://github.com/<your-username>/unity-3d-game.git
   ```
2. **Open in Unity Hub** -- click *Open* and select the cloned folder
3. **Wait** for Unity to import all assets (first launch may take a minute)
4. **Open Level 1** -- go to `Assets/Scenes/SampleScene.unity`
5. **Press Play** in the Unity Editor

### Generating Levels via Editor Tools
The project includes custom editor scripts to rebuild levels from scratch:
- `Semana9 > Configurar Escena Semana 9` -- rebuilds Level 1
- `Semana9 > Crear Nivel 2` -- generates Level 2
- `Semana9 > Crear Nivel 3` -- generates Level 3

> **Note:** Make sure all three scenes are added to **File > Build Settings** in the correct order (SampleScene, Nivel2, Nivel3) for scene transitions to work.

---

## :video_game: Controls

| Action | Key |
|--------|-----|
| Move | `W` `A` `S` `D` |
| Look around | Mouse |
| Jump | `Space` |
| Sprint | `Left Shift` (hold) |
| Interact | `E` |
| Retry (on death) | `R` |

---

## :camera: Screenshots

<!-- Add your screenshots here -->
| Level 1 | Level 2 | Level 3 |
|---------|---------|---------|
| ![Level 1](docs/screenshots/level1.png) | ![Level 2](docs/screenshots/level2.png) | ![Level 3](docs/screenshots/level3.png) |

> Replace the placeholder paths above with actual screenshots of your game.

---

## :brain: Architecture Highlights

- **Event-driven collectible system** -- `Coleccionable.OnRecogido` (static Action) decouples spawners from collectibles
- **Inheritance-based interactions** -- all interactables extend `ObjetoInteractivo`, keeping the `E`-key trigger logic in one place
- **Static game state** -- `GestorJuego` provides a lightweight, scene-independent state manager with `Reiniciar()` for clean resets
- **Editor automation** -- full scenes (geometry, lighting, UI, scripts, materials) are generated entirely from C# editor scripts, making them reproducible and version-controllable
- **Platform transport system** -- `MovimientoPingPongLineal` includes a trigger-based detection system that moves `CharacterController` and `Rigidbody` objects along with the platform

---

## :handshake: Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## :page_facing_up: License

This project is available under the [MIT License](LICENSE).

---

<p align="center">
  Built with Unity 2022.3 LTS &bull; Made with determination
</p>
