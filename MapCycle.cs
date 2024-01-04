﻿using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace MapCycle;

// Class to fetch data from the json config
public class ConfigGen : BasePluginConfig
{
    [JsonPropertyName("Maps")]
    public List<MapItem> Maps { get; set; } = new List<MapItem>
    {
        new MapItem { Name = "de_dust2", Id = "de_dust2", Workshop = false },
        new MapItem { Name = "de_aztec", Id = "3070960099", Workshop = true }
    };

    [JsonPropertyName("Randomize")]
    public bool Randomize { get; set; } = false;
}

// Class to instanciate a map item with its id and name
public class MapItem
{
    public string Name { get; set; }
    public string Id { get; set; }
    public bool Workshop { get; set; }
}

// Main class of the plugin
public class MapCycle : BasePlugin, IPluginConfig<ConfigGen>
{
    // plugin informations
    public override string ModuleName => "MapCycle";
    public override string ModuleAuthor => "NANOR";
    public override string ModuleVersion => "1.0.4";

    // plugin configs
    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) { Config = config; }

    // private variables
    private MapItem? _nextMap;

    private string? _mapCycleStringTitle;
    private string? _nextMapString;
    private string? _nextCustomMapString;
    private string? _notExistingMapString;
    private MapItem? _currentMap;
    private Random _randomIndex = new Random();

    // Load the plugin
    public override void Load(bool hotReload)
    {
        _mapCycleStringTitle = $" {ChatColors.Red}[Map Cycle]{ChatColors.Default}";
        _nextMapString = $"{_mapCycleStringTitle} The next map is:";
        _nextCustomMapString = $"{_mapCycleStringTitle} The next map is now:";
        _notExistingMapString = $"{_mapCycleStringTitle} This map doesn't exist in the map cycle:";

        // Set the next map on map start
        RegisterListener<Listeners.OnMapStart>(SetNextMap);

        AddCommand("mc_nextmap?", "Get the next map of the cycle", OnGetNextMapCommand);

        if (hotReload){
            SetNextMap(Server.MapName);
        }

        // Print the next map on map start
        PrintNextMapOnMapStart();

        // Create the timer to change the map
        RegisterEventHandler<EventCsWinPanelMatch>((@event, info) =>
        {
            AutoMapCycle();
            return HookResult.Continue;
        });
    }

    [ConsoleCommand("mc_nextmap", "Set the next map of the cycle")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, usage: "<#map name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnSetNextCommand(CCSPlayerController? caller, CommandInfo info)
    {
        var commandMapName = info.GetArg(1);
        var map = Config.Maps.FirstOrDefault(x => x.Name == commandMapName);
        if (map == null)
        {
            info.ReplyToCommand($"{_notExistingMapString} {commandMapName}");
            return;
        } else {
            _nextMap = map;
            info.ReplyToCommand($"{_nextCustomMapString} {_nextMap.Name}");
        }
    }

    [ConsoleCommand("mc_go", "Direct switch to the map you want of the cycle")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper( whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnGoToNextMapCommand(CCSPlayerController? caller, CommandInfo info)
    {
        ChangeMap();
    }

    [ConsoleCommand("mc_goto", "Direct switch to the next map of the cycle")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, usage: "<#map name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnGoToNextMapNamedCommand(CCSPlayerController? caller, CommandInfo info)
    {
        var commandMapName = info.GetArg(1);
        var map = Config.Maps.FirstOrDefault(x => x.Name == commandMapName);
        if (map == null)
        {
            // If the map doesn't exist, we print an error
            info.ReplyToCommand($"{_notExistingMapString} {commandMapName}");
            return;
        }
        else
        {
            // Else we change the map
            _nextMap = map;
            ChangeMap();
        }
    }

    public void OnGetNextMapCommand(CCSPlayerController? caller, CommandInfo info)
    {
        // Print the next map
        info.ReplyToCommand($"{_nextMapString} {_nextMap.Name}");
    }

    private void PrintNextMapOnMapStart()
    {
        // Print the next map on map start
        AddTimer(10, () => {
            Server.PrintToChatAll($"{_nextMapString} {_nextMap.Name}");
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void AutoMapCycle()
    {
        // Print the next map
        Server.PrintToChatAll($"{_nextMapString} {_nextMap.Name}");
        // Change the map
        AddTimer(10f, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeMap()
    {
        // If the next map is a workshop map, we use the host_workshop_map command
        if (_nextMap.Workshop)
        {
            Server.ExecuteCommand($"host_workshop_map {_nextMap.Id}");
        }
        else
        {
            // Else we use the map command
            Server.ExecuteCommand($"map {_nextMap.Name}");
        }
    } 

    private void SetNextMap(string mapName)
    {
        // By default, we set the first map of the cycle
        _nextMap = Config.Maps[0];
        // Get the next map index
        var _nextIndex = CurrentMapIndex() + 1;

        // If the randomize option is enabled, we set a random map
        if (Config.Randomize)
        {
            do {
                _nextIndex = _randomIndex.Next(0, Config.Maps.Count);
            } while (_nextIndex == CurrentMapIndex());
        }

        // If the next map index is greater than the map cycle count, we let the first map of the cycle
        if (_nextIndex < Config.Maps.Count){
            _nextMap = Config.Maps[_nextIndex];
        }
    }

    private MapItem? CurrentMap()
    {
        // If the current map is the same as the server map, we return it
        if(_currentMap != null && _currentMap.Name == Server.MapName) {
            Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] ****************************{_currentMap.Name}/ {Server.MapName}******************************");
            return _currentMap;
        } else
        {
            // Else we search the current map in the map cycle
            _currentMap = Config.Maps.FirstOrDefault(x => x.Name == Server.MapName);
            // If the current map doesn't exist in the map cycle, we print an error
            if (_currentMap == null)
            {
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] ****************************ERROR MAP CYCLE******************************");
                Server.PrintToChatAll($" [MapCycle] The current map doesn't exist in the map cycle: {Server.MapName}");
                Server.PrintToChatAll($" [MapCycle] Please check that the map is correcly named in the json config.");
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] ****************************ERROR MAP CYCLE******************************");
            }
            return _currentMap;
        }
    }

    private int CurrentMapIndex()
    {
        if(CurrentMap() == null) {
            return 0;
        } else {
            return Config.Maps.FindIndex(a => a.Name == CurrentMap().Name);
        }
    }
}
