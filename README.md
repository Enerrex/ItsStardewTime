# ItsStardewTime

Controls for the passage of time in Stardew Valley! This SMAPI mod allows you to customize the length of in-game minutes, 
freeze time under various conditions, and provides advanced multiplayer pause synchronization features.

## Overview
ItsStardewTime replaces and enhances features found in older mods like `PauseInMultiplayer` and `TimeSpeed`. It allows for:
- **Custom Time Speed**: Adjust how many real-world seconds make up an in-game minute (default is 0.7s, standard is ~0.7s but varies).
- **Time Freezing**: Automatically freeze time in specific locations (like mines or indoors) or after a certain time of day.
- **Multiplayer Pause**: Voting system for pausing the game in multiplayer, with various modes (Fair, Host, etc.).
- **Clock Customization**: Show/hide minutes, use 24-hour format, and display pause indicators.
- **Smart Integration**: Automatically merges configurations from legacy mods if detected.

## Requirements
- [Stardew Valley](https://www.stardewvalley.net/)
- [SMAPI](https://smapi.io/) (Stardew Modding API) 3.0.0 or higher

## Setup
1.  **Install SMAPI**: Follow the [official instructions](https://smapi.io/#install).
2.  **Download the mod**: Extract the `ItsStardewTime` folder into your `Stardew Valley/Mods` directory.
3.  **Run the game**: Launch the game using the SMAPI executable.

## Configuration
The mod generates a `config.json` file in its folder after the first run. 
You can edit this file directly or use the [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (recommended) to adjust settings in-game.

### Key Bindings (Default)
- **Pause Vote**: `Pause` key (can be remapped).
- **Freeze Time**: `None` (must be configured in `config.json` or GMCM).
- **Increase/Decrease Time Speed**: `None` (must be configured).

### Configuration Options
| Option | Description |
| :--- | :--- |
| `EnableOnFestivalDays` | Whether time scaling applies during festivals. |
| `SecondsPerMinute` | Base time speed (seconds per in-game minute). |
| `FreezeTime` | Locations and times where the game automatically freezes. |
| `ObjectsPassTimeWhenTimeIsFrozen` | If crops/machines should still process while time is frozen. |
| `Use24HourFormat` | Toggles between 12h and 24h clock display. |
| `PauseMode` | Multiplayer pause logic (`Fair`, `HostOnly`, etc.). |

## Development
This mod is built using .NET 6.0 and SMAPI's build config.

### Project Structure
- `Framework/`: Core logic for time control, multiplayer states, and notifications.
- `Patches/`: Harmony patches for modifying game behavior (Skull Cavern, Festivals, Clock UI).
- `i18n/`: Translation files for multiple languages (DE, ES, FR, KO, PT, RU, ZH).
- `TimeMaster.cs`: Main entry point for the mod.

### Scripts & Build
- **Build**: Use `dotnet build` or your preferred IDE (Rider/Visual Studio).
- **Tests**: No automated test suite currently implemented. TODO: Add unit tests for time calculation logic.

## License
TODO: Add license information (e.g., MIT, GPL).

---
*Created by Caboose Sage.*
