# MapCycle Plugin for CounterStrikeSharp

## Overview
MapCycle is a plugin designed for CounterStrikeSharp. This plugin enables server administrators to automate the rotation of a predefined list of maps. It's compatible with both standard and workshop maps.

## Features
- **Automatic Map Rotation**: Rotates maps according to a configurable list.
- **Support for Multiple Map Types**: Compatible with both standard and workshop maps.
- **Simple Configuration**: Uses a JSON file for easy setup.
- **Chat Notifications**: Informs players about the upcoming map.

## Commands
- **`mc_nextmap?` / `mc_nextmap de_dust2` (SERVER CONSOLE)**: Use `mc_nextmap?` to check the upcoming map. To set the next map, input `mc_nextmap de_dust2`.
- **`!mc_nextmap?` / `!mc_nextmap de_dust2` (CHAT)**: Enter `!mc_nextmap?` to view the next map. To select a different map, type `!mc_nextmap de_dust2`.
- **`!mc_goto de_dust2`**: This command allows direct access to a chosen map (`de_dust2` in this case) in your cycle, bypassing the need to wait for the current match to end.
- **`!mc_go`**: Use this command to immediately transition to the next map, without the need to wait for the current match to conclude.

## Installation
- Download the latest release from [here](https://github.com/RonanLOUARN/Map-Cycle/releases/tag/release).
- Unzip the folder named `MapCycle`.
- Place it in the directory: `game/csgo/addons/counterstrikesharp/plugins/`.
- You can start your server with the two default maps, or add new maps to the configuration file, which will be automatically generated upon the first startup. 
- This plugins needs you to have access of the flag `@css/changemap` in `game/csgo/addons/counterstrikesharp/configs/admins.json` 

## Admin config
Go to `game/csgo/addons/counterstrikesharp/configs/admins.json`. Create the file if needed.
Write the configuration like the following one.
```json
{
  "NANOR": {
    "identity": "STEAM_0:0:123123123",
    "flags": ["@css/changemap"]
  }
}

```

More informations [here](https://docs.cssharp.dev/docs/admin-framework/defining-admins.html)


## Configuration
The configuration file is automatically generated in `game/csgo/addons/counterstrikesharp/configs/plugins/MapCycle`, initially containing two default maps.

`Randomize` to play the maps in random order.

Each map in the configuration file includes the following attributes:
- `Name`: The actual name of the map (e.g., `de_dust2`, `de_cbble`).
- `Id`: The workshop ID, or the map name again if it's an official map.
- `Workshop`: Indicates whether the map is from the workshop (`true` or `false`).

### Example Configuration
```json
{
  "Randomize": true,
  "Maps": [
    {
      "Name": "de_dust2",
      "Id": "de_dust2",
      "Workshop": false
    },
    {
        "Name": "cs_asssault",
        "Id": "3070594412",
        "Workshop": true
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

## Dependencies
- CounterStrikeSharp.API
- System.Text.Json

## Author
- ModuleName: MapCycle
- ModuleAuthor: NANOR
- ModuleVersion: 1.0.3

## Support
For assistance, please raise an issue on the GitHub repository of the project.

---

**Note:** It is essential to ensure that CounterStrikeSharp and all necessary dependencies are properly installed and configured on your server to guarantee the effective functioning of the MapCycle plugin.

