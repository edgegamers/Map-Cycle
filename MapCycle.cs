using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;

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
    public override string ModuleVersion => "1.0.0";

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

    // Load the plugin
    public override void Load(bool hotReload)
    {
        _mapCycleStringTitle = $" {ChatColors.Lime}[Map Cycle]{ChatColors.LightBlue}";
        _nextMapString = $"{_mapCycleStringTitle} The next map is:";
        _nextCustomMapString = $"{_mapCycleStringTitle} The next map is now:";
        _notExistingMapString = $"{_mapCycleStringTitle} This map doesn't exist in the map cycle:";

        // Set the next map on map start
        RegisterListener<Listeners.OnMapStart>(SetNextMap);

        if(hotReload){
            SetNextMap(Server.MapName);
        }

        // Create the command to get/set the next map
        CreateNextMapCommand();

        // Print the next map on map start
        PrintNextMapOnMapStart();

        // Create the timer to change the map
        RegisterEventHandler<EventCsWinPanelMatch>((@event, info) =>
        {
            AutoMapCycle();
            return HookResult.Continue;
        });
    }

    private void CreateNextMapCommand()
    {
        AddCommand("mc_nextmap", "", (player, commandInfo) =>
        {

            var commandMapName = commandInfo.GetArg(1);

            if (commandMapName.Length == 0)
            {
                commandInfo.ReplyToCommand($"{_nextMapString} {_nextMap.Name}");
                return;
            }

            var map = Config.Maps.FirstOrDefault(x => x.Name == commandMapName);

            if (map == null)
            {
                commandInfo.ReplyToCommand($"{_notExistingMapString} {commandMapName}");
                return;
            } else {
                _nextMap = map;
                Server.PrintToChatAll($"{_nextCustomMapString} {_nextMap.Name}");
                commandInfo.ReplyToCommand($"{_nextCustomMapString} {_nextMap.Name}");
            }
        });
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

        foreach (var map in Config.Maps)
        {
            if (map.Name == mapName)
            {
                // If there is a map after the current one, we set it as next map
                if(_iterationIndex + 1 < Config.Maps.Count)
                {
                    _nextMap = Config.Maps[_iterationIndex + 1];
                }
                break;
            }
            _iterationIndex++;
        }

        _iterationIndex = 0;
    }
}
