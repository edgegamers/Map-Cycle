# MapCycle Plugin for CounterStrikeSharp

## Overview
MapCycle is a plugin designed for CounterStrikeSharp. This plugin enables server administrators to automate the rotation of a predefined list of maps. It's compatible with both standard and workshop maps.

## Donate
I dedicate a significant part of my free time to coding and developing meaningful plugins for CS2. If you appreciate my work and would like to support me, please consider making a donation through PayPal. Your support helps me continue coding CS2 plugins. Thank you!
[![PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=MVCFKC7V772WS)

## Features
- **Automatic Map Rotation**: Rotates maps according to a configurable list.
- **Support for Multiple Map Types**: Compatible with both standard and workshop maps.
- **Simple Configuration**: Uses a JSON file for easy setup.
- **Chat Notifications**: Informs players about the upcoming map.

## Compatibility
- [CS2 Simple Admin](https://github.com/daffyyyy/CS2-SimpleAdmin)

## Commands
### Map Cycle
1) **`!addmap cs_assault Assault 3070594412`**: This command allows you to add a new map to the cycle without having to manually edit the configuration file.
- Pattern: `!addmap mapname display_name workshopid_or_mapname_for_offi_map`
- E.g WS map:  `!addmap cs_assault Assault 3070594412`
- E.g official map:  `!addmap de_dust2 "Dust 2" de_dust2`

---

2) **`!removemap cs_assault`**: This command allows you to remove a map from the cycle directly in-game or from the console, eliminating the need to manually edit the configuration file.
- Pattern: `!removemap mapname`
- E.g WS map: `!removemap cs_assault`
- E.g official map: `!removemap de_dust2`

---

3) **`!keepmap Assault`**: This command allows you to add the current map to the cycle.
- Pattern: `!keepmap [optional]mapDisplayName`

It automatically detects if it's a workshop map or not.
How to use it:
- For a workshop map (`!goto 123123123`): You can type the map ID directly with the `goto` command, even if it's not in the cycle. Once you're on the map, if you enjoy it, you can type `!keepmap` or `!keepmap NoobMap` with the display name or not. By default, the display name will be the map name.
- For an official map (`!goto de_inferno`): You can type the map name directly with the `goto` command, even if it's not in the cycle. Once you're on the map, if you enjoy it, you can type `!keepmap` or `!keepmap Inferno` with the display name or not. By default, the display name will be the map name.

---

4) **`!nextmap` / `!nextmap de_dust2`**: Enter `!nextmap` to view the next map. To select a different map, type `!nextmap de_dust2` or `!nextmap de_aztec`.

---

5) **`!goto cs_assault`**: This command allows direct access to a chosen map (`de_dust2` in this case) in your cycle, bypassing the need to wait for the current match to end. You can also go to a map workshop ID using the command `!goto 123123123`, even if it's not in the cycle.

---

6) **`!go`**: Use this command to immediately transition to the next map, without the need to wait for the current match to conclude.


**`mc_` commands are deprecated **

### RTV

#### New commands
Use the ChatMenu to vote. The !mc_vote command no longer exists. To vote, simply press !1, or !2, or !3 etc...
`mc_` commands are deprecated.

![img](https://drive.google.com/uc?export=view&id=18yyRQb2Z5mfOI7a_mkhCcudb8c0Tq_UJ)

## Installation
- Download the latest release from [here](https://github.com/RonanLOUARN/Map-Cycle/releases).
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

**JSON attributes**

`Randomize` to play the maps in random order.

`RtvEnabled` Enable or disable the vote.

`RtvMapCount` Number of proposed maps. They are randomly selected in the cycle.

`RtvRoundStartVote` Number of the round the vote start. E.g 5 the vote will at the beginning of the fifth round (warmup not included).

`RtvDurationInSeconds` Voting duration

`RtvStartVoteAtTheEnd` If you activate this option, voting will start at the end of the match. And the map will change at the end of the win panel and will no longer take into account the round and duration options for RTV.

`RtvVoteRatio` Determines the minimum ratio of votes required for a vote to be considered valid.

Each map in the configuration file includes the following attributes:
- `Name`: The actual name of the map (e.g., `de_dust2`, `de_cbble`).
- `DisplayName`: The name that is displayed in the chat. If this field is empty, it will display the name of the map (de_dust2, etc..).
- `Id`: The workshop ID, or the map name again if it's an official map.
- `Workshop`: Indicates whether the map is from the workshop (`true` or `false`).

### Example Configuration
```json
{
  "Randomize": false,
  "RtvEnabled": true,
  "RtvMapCount": 5,
  "RtvRoundStartVote": 1,
  "RtvDurationInSeconds": 30,
  "RtvStartVoteAtTheEnd": true,
  "RtvVoteRatio": 0.5,
  "Maps": [
    {
      "Name": "de_dust2",
      "DisplayName": "Dust 2",
      "Id": "de_dust2",
      "Workshop": false
    },
    {
        "Name": "cs_assault",
        "DisplayName": "Assault",
        "Id": "3070594412",
        "Workshop": true
    },
    {
      "Name": "de_lake",
      "DisplayName": "Lake",
      "Id": "3070563536",
      "Workshop": true
    },
    {
      "Name": "de_cbble",
      "DisplayName": "Cobblestone",
      "Id": "3070293560",
      "Workshop": true
    }
  ],
  "ConfigVersion": 1 // Do not touch
}

```

## Troubleshooting
### If the Cycle doesn't work as expected and it restarts to the first map
1) **Check if the JSON config is correctly formated.** You can use this tool to verify that: https://jsonformatter.curiousconcept.com/
If it's correctly formated then you'll have a green screen

![img](https://drive.google.com/uc?export=view&id=1rzZxelI_hmk1yVMJICXckw6eBBJmfGyx)

If not, you'll have a red screen with explanations:

![img](https://drive.google.com/uc?export=view&id=1RW5HQ8jc363xEVKZzrJwC3Lvk0YOPmLv)


2) **Check if the map name is correct.**
Set the map on your server with `host_workshop_map 123123123` and when the map has started you verify the name directly on the steam server browser.
![img](https://drive.google.com/uc?export=view&id=1vJllKCRsX9oUR9HC4yNrzkE45jxM8NnL)

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

## Author
- ModuleName: MapCycle
- ModuleAuthor: NANOR
- ModuleVersion: 1.3.2

## Support
For assistance, please raise an issue on the GitHub repository of the project.

---

**Note:** It is essential to ensure that CounterStrikeSharp and all necessary dependencies are properly installed and configured on your server to guarantee the effective functioning of the MapCycle plugin.

