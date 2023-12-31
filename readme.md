# MapCycle Plugin for CounterStrikeSharp

## Overview
MapCycle is a plugin designed for CounterStrikeSharp. This plugin enables server administrators to automate the rotation of a predefined list of maps. It's compatible with both standard and workshop maps.

## Features
- **Automatic Map Rotation**: Rotates maps according to a configurable list.
- **Support for Multiple Map Types**: Compatible with both standard and workshop maps.
- **Simple Configuration**: Uses a JSON file for easy setup.
- **Chat Notifications**: Informs players about the upcoming map.
- **Command to manually get/set the next map**: To view the upcoming map, enter the command `mc_nextmap`. To specify the next map, use `mc_nextmap de_dust2`.

## Installation
- Download the latest release from [here](https://github.com/RonanLOUARN/Map-Cycle/releases/tag/release).
- Unzip the folder named `MapCycle`.
- Place it in the directory: `game/csgo/addons/counterstrikesharp/plugins/`.
- You can start your server with the two default maps, or add new maps to the configuration file, which will be automatically generated upon the first startup.

## Configuration
The configuration file is automatically generated in `game/csgo/addons/counterstrikesharp/configs/plugins/MapCycle`, initially containing two default maps.

Each map in the configuration file includes the following attributes:
- `Name`: The actual name of the map (e.g., `de_dust2`, `de_cbble`).
- `Id`: The workshop ID, or the map name again if it's an official map.
- `Workshop`: Indicates whether the map is from the workshop (`true` or `false`).

### Example Configuration
```json
{
  "Maps": [
    {
      "Name": "de_dust2",
      "Id": "de_dust2",
      "Workshop": false
    },
    {
      "Name": "de_lake",
      "Id": "3070563536",
      "Workshop": true
    },
    {
      "Name": "de_cbble",
      "Id": "3070293560",
      "Workshop": true
    }
  ],
  "ConfigVersion": 1
}

```

### ConfigGen Class
- `Maps`: A list of `MapItem` objects, with each object representing a map in the rotation cycle.

### MapItem Class
- `Name`: The name of the map.
- `Id`: The map's unique ID, used for workshop maps.
- `Workshop`: A boolean value indicating whether the map is sourced from the workshop (`true`) or is a standard map (`false`).

## Usage
1. Configure your map cycle in the provided configuration file.
2. Install and activate the plugin on your CounterStrikeSharp server.
3. The plugin will automatically manage map rotation, announcing the next map to players at the conclusion of each match.

## Plugin Methods
- `Load(bool hotReload)`: Sets up the plugin and initiates event handlers.
- `ChangeMap()`: Transitions the game to the subsequent map in the rotation.
- `SetNextMap()`: Selects the next map in the cycle, based on the currently active map.

## Dependencies
- CounterStrikeSharp.API
- System.Text.Json

## Author
- ModuleName: MapCycle
- ModuleAuthor: NANOR
- ModuleVersion: 0.0.1

## Support
For assistance, please raise an issue on the GitHub repository of the project.

---

**Note:** It is essential to ensure that CounterStrikeSharp and all necessary dependencies are properly installed and configured on your server to guarantee the effective functioning of the MapCycle plugin.

