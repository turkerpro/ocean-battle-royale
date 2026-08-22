# Ocean Battle Royale

3D Multiplayer Ocean Battle Royale - Low Poly, 50+ Players, Photon Fusion, PlayFab

## Tech Stack
- **Engine**: Unity 2022.3 LTS
- **Networking**: Photon Fusion 2 (Shared Mode)
- **Backend**: PlayFab (Free Tier)
- **Rendering**: URP + Custom Ocean Shader
- **Platforms**: Android, WebGL, Windows

## Project Structure
```
Assets/_Project/
├── Scripts/
│   ├── Core/           # GameManager, NetworkManager
│   ├── Network/        # NetworkedShip, Input handling
│   ├── Ship/           # ShipPhysics, progression
│   ├── Combat/         # Weapons, mines, damage
│   ├── World/          # Ocean, spawn, boundaries
│   └── UI/             # HUD, mobile controls, shop
├── Shaders/            # OceanShader (Gerritsen waves)
├── Scenes/             # MainMenu, Prototype, Gameplay
├── Resources/          # Settings, Prefabs
└── ScriptableObjects/  # Data configs
```

## Development Phases
- **Phase 0** (Week 1): Setup, Network, Physics, Ocean, 50-bot test
- **Phase 1** (Week 2-3): Network Foundation, Lobby, Movement
- **Phase 2** (Week 4-5): Ship System, Progression, Tier Upgrades
- **Phase 3** (Week 6-7): Combat Core, Weapons, Mines
- **Phase 4** (Week 8-9): World, Polish, Mobile UI, VFX/SFX
- **Phase 5** (Week 10): Cosmetic Shop, IAP
- **Phase 6** (Week 11-12): Scale Test, Launch Prep

## Getting Started
1. Open in Unity 2022.3.20f1+
2. Add Photon AppId to `Assets/_Project/Resources/PhotonAppSettings.asset`
3. Add PlayFab TitleId to `Assets/_Project/Resources/PlayFabSettings.asset`
4. Open `Scenes/Prototype.unity` and press Play

## Build Commands
```bash
# Android (AAB)
Unity -batchmode -executeMethod BuildScript.BuildAndroid

# WebGL
Unity -batchmode -executeMethod BuildScript.BuildWebGL

# Windows
Unity -batchmode -executeMethod BuildScript.BuildWindows
```

## CI/CD
GitHub Actions workflow in `.github/workflows/ci.yml` runs on every push:
- Unit + Integration tests
- Android AAB build
- WebGL build
- Windows Standalone build

## License
MIT
