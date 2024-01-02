using System.Text.Json.Serialization;
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
    public override string ModuleVersion => "1.0.3";

    // plugin configs
    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) { Config = config; }

    // private variables
    private int _iterationIndex = 0;
    private MapItem? _nextMap;

    private string? _mapCycleStringTitle;
    private string? _nextMapString;
    private string? _nextCustomMapString;
    private string? _notExistingMapString;
    private MapItem? _currentMap;

    // Load the plugin
    public override void Load(bool hotReload)
    {
        _mapCycleStringTitle = $" {ChatColors.Red}[Map Cycle]{ChatColors.Default}";
        _nextMapString = $"{_mapCycleStringTitle} The next map is:";
        _nextCustomMapString = $"{_mapCycleStringTitle} The next map is now:";
        _notExistingMapString = $"{_mapCycleStringTitle} This map doesn't exist in the map cycle:";

        // Set the next map on map start
        RegisterListener<Listeners.OnMapStart>(SetNextMap);

        if(hotReload){
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

    [ConsoleCommand("mc_nextmap?", "Get the next map of the cycle")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnGetNextMapCommand(CCSPlayerController? caller, CommandInfo info)
    {
        info.ReplyToCommand($"{_nextMapString} {_nextMap.Name}");
    }

    private void PrintNextMapOnMapStart()
    {
        AddTimer(10f, () => {
            Server.PrintToChatAll($"{_nextMapString} {_nextMap.Name}");
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void AutoMapCycle()
    {
        Server.PrintToChatAll($"{_nextMapString} {_nextMap.Name}");
        AddTimer(10f, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeMap()
    {
        if (_nextMap.Workshop)
        {
            Server.ExecuteCommand($"host_workshop_map {_nextMap.Id}");
        }
        else
        {
            Server.ExecuteCommand($"map {_nextMap.Name}");
        }
    } 

    private void SetNextMap(string mapName)
    {
        // By default, we set the first map of the cycle
        _nextMap = Config.Maps[0];

        // Get the next map index
        var _nextIndex = CurrentMapIndex() + 1;

        // If the next map index is greater than the map cycle count, we let the first map of the cycle
        if (_nextIndex < Config.Maps.Count){
            _nextMap = Config.Maps[_nextIndex];
        }
    }

    private MapItem CurrentMap()
    {
        if(_currentMap != null && _currentMap.Name == Server.MapName) {
            return _currentMap;
        } else
        {
            _currentMap = Config.Maps.FirstOrDefault(x => x.Name == Server.MapName);
            if (_currentMap == null)
            {
                Server.PrintToConsole($" {ChatColors.Red}[MapCycle] ****************************ERROR MAP CYCLE******************************");
                Server.PrintToConsole($" [MapCycle] The current map doesn't exist in the map cycle: {Server.MapName}");
                Server.PrintToConsole($" [MapCycle] Please check that the map is correcly named in the json config.");
                Server.PrintToConsole($" {ChatColors.Red}[MapCycle] ****************************ERROR MAP CYCLE******************************");
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
