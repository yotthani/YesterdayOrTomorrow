# TrekGame - Galactic Strategy

A turn-based 4X strategy game inspired by Star Trek and classic games like Birth of the Federation.

## Version
v1.34.0 "Asset Pipeline"

## Features
- 6+ playable factions (Federation, Klingon, Romulan, Cardassian, Ferengi, Borg)
- Galaxy exploration with procedural star systems
- Colony management with building construction
- Fleet command and ship management
- Research and technology trees
- Diplomatic relations

## New: Asset Generator Tool
Use the Asset Generator to create consistent sprite sheets for all factions:

```bash
cd src/Tools/AssetGenerator
dotnet run
```

## Getting Started

```bash
# Clone and restore
git clone https://github.com/yot-community/trekgame.git
cd trekgame
dotnet restore

# Run the server
cd src/Presentation/Server
dotnet run

# Open browser at https://localhost:7001
```

## Project Structure
- `src/Core/Domain` - Game logic and entities
- `src/Core/Application` - DTOs and interfaces
- `src/Infrastructure` - Repositories and auth
- `src/Presentation/Server` - ASP.NET Core API
- `src/Presentation/Web` - Blazor WebAssembly UI
- `src/Tools/AssetGenerator` - Sprite sheet generator

## Asset Specifications
See `docs/ASSET_SPECIFICATION.md` for sprite sheet grid specs.

## License
Fan project for educational purposes. Star Trek™ is a trademark of CBS Studios Inc.
