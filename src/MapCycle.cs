using System.Globalization;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Core.Translations;

using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;


namespace MapCycle;

// Class to fetch data from the json config
[MinimumApiVersion(80)]
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

    [JsonPropertyName("RtvEnabled")]
    public bool RtvEnabled { get; set; } = false;

    [JsonPropertyName("RtvMapCount")]
    public int RtvMapCount { get; set; } = 5;

    [JsonPropertyName("RtvRoundStartVote")]
    public int RtvRoundStartVote { get; set; } = 3;

    [JsonPropertyName("RtvDurationInSeconds")]
    public int RtvDurationInSeconds { get; set; } = 30;
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
    public override string ModuleVersion => "1.2.0";

    // plugin configs
    public ConfigGen Config { get; set; } = null!;
    public bool VoteCountNeededPercent { get; private set; }

    public void OnConfigParsed(ConfigGen config) { Config = config; }

    // private variables
    private MapItem? _nextMap;

    private int _currentRound = 0;
    private MapItem? _currentMap;
    private Random _randomIndex = new Random();
    private Rtv? _rtv;

    // Load the plugin
    public override void Load(bool hotReload)
    {
        // Set the next map on map start
        RegisterListener<Listeners.OnMapStart>(SetNextMap);
        LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMapNow", "OK");
        if (hotReload){
            Server.PrintToConsole($"[MapCycle] {ChatColors.Default}Hot reload detected, the next map will be the same as before the reload {Config.RtvRoundStartVote}");
            if (!Config.RtvEnabled)
            {
                SetNextMap(Server.MapName);
            } else {
                InitRTV();
            }
        } else {
            if (!Config.RtvEnabled)
            {
                SetNextMap(Server.MapName);
            } else {
                InitRTV();
            }
        }

        if (Config.RtvEnabled)
        {
            //AddCommand("mc_vote", "Get the next map of the cycle", _rtv.AddVote);
            
            // Add the event to change the map when the vote is finished
            _rtv.EndVoteEvent += (sender, e) =>
            {
                // To avoid a new rtv trigger
                _currentRound = -100;

                if (_rtv.NextMap != null){
                    _nextMap = _rtv.NextMap;
                } else {
                    SetNextMap(Server.MapName);
                }
                LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMapNow", _nextMap.Name);
            };
        }

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            _currentRound++;
            if (Config.RtvEnabled && _currentRound == Config.RtvRoundStartVote + 1) // +1 for the warmup
            {
                _rtv.Call();
            }
            return HookResult.Continue;
        });

        // Create the timer to change the map
        RegisterEventHandler<EventCsWinPanelMatch>((@event, info) =>
        {
            AutoMapCycle();
            return HookResult.Continue;
        });
    }

    public void InitRTV()
    {
        if(_rtv == null){
            _rtv = Rtv.Instance;
            _rtv.Config = Config;
            _rtv.Localizer = Localizer;
        }
    }

    [ConsoleCommand("mc_nextmap", "Gets/sets the next map of the cycle")]
    public void OnSetNextCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if(info.ArgCount == 1 || !AdminManager.PlayerHasPermissions(caller, "@css/changemap")) {
            OnGetNextMapCommand(caller, info);
            return;
        }
        var commandMapName = info.GetArg(1);
        var map = Config.Maps.FirstOrDefault(x => x.Name == commandMapName);
        if (map == null)
        {
            info.ReplyLocalized(Localizer, "NotExistingMap", commandMapName);
            return;
        } else {
            _nextMap = map;
            info.ReplyLocalized(Localizer, "NextMapNow", _nextMap.Name);
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
            info.ReplyLocalized(Localizer, "NotExistingMap", commandMapName);
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
        if(_nextMap == null) {
            info.ReplyLocalized(Localizer, "NextMapUnset");
        } else {
            info.ReplyLocalized(Localizer, "NextMap", _nextMap.Name);
        }
    }

    private void AutoMapCycle()
    {
        if (_rtv != null && _rtv.NextMap != null)
        {
            _nextMap = _rtv.NextMap;
        }
        // Print the next map
        LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMap", _nextMap.Name);
        // Change the map
        AddTimer(10f, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeMap()
    {
        if(_rtv != null){
            _rtv.NextMap = null;
        }
        _currentRound = 0;

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
            return _currentMap;
        } else
        {
            // Else we search the current map in the map cycle
            _currentMap = Config.Maps.FirstOrDefault(x => x.Name == Server.MapName);
            // If the current map doesn't exist in the map cycle, we print an error
            if (_currentMap == null)
            {
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}****************************ERROR MAP CYCLE******************************");
                Server.PrintToChatAll($" [MapCycle] The current map doesn't exist in the map cycle: {Server.MapName}");
                Server.PrintToChatAll($" [MapCycle] Please check that the map is correcly named in the json config.");
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}****************************ERROR MAP CYCLE******************************");
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
