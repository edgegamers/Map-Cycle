using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;


namespace MapCycle;

[MinimumApiVersion(80)]

// Main class of the plugin
public class MapCycle : BasePlugin, IPluginConfig<ConfigGen>
{
    // plugin informations
    public override string ModuleName => "MapCycle";
    public override string ModuleAuthor => "NANOR";
    public override string ModuleVersion => "1.3.1";

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
                LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMapNow", _nextMap.DName());
            };
        }

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            _currentRound++;
            if (Config.RtvEnabled && _currentRound == Config.RtvRoundStartVote + 1 && !Config.RtvStartVoteAtTheEnd) // +1 for the warmup
            {
                _rtv.Call(Config!.RtvDurationInSeconds);
            }
            return HookResult.Continue;
        });

        // Create the timer to change the map
        RegisterEventHandler<EventCsWinPanelMatch>((@event, info) =>
        {
            // Start the vote at the end of the match
            if(Config.RtvStartVoteAtTheEnd && Config.RtvEnabled)
            {
                _rtv.Call(15);
            }
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

    [ConsoleCommand("addmap", "Add a new map in the cycle")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 3, usage: "<#map name> <#display name> <#id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnAddMap(CCSPlayerController? caller, CommandInfo info)
    {
        if(info.ArgCount < 3) {
            info.ReplyLocalized(Localizer, "NotEnoughArgs", 3, info.ArgCount);
            return;
        }

        var mapName = info.GetArg(1);
        var displayName = info.GetArg(2);
        var id = info.GetArg(3);
        Server.PrintToChatAll($"[MapCycle] {ChatColors.Default}Adding map {mapName} with display name {displayName} and id {id}");
        bool workshop = true;

        if (Config.Maps.Any(x => x.Name == mapName))
        {
            info.ReplyLocalized(Localizer, "AlreadyExistingMap", mapName);
            return;
        }

        if(id == mapName)
        {
            workshop = false;
        }

        Config.AddMap(mapName, displayName, id, workshop);
        info.ReplyLocalized(Localizer, "MapAdded", mapName);
    }

    [ConsoleCommand("removemap", "Remove a map from the cycle")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, usage: "<#map name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnRemoveMap(CCSPlayerController? caller, CommandInfo info)
    {
        if (info.ArgCount < 1)
        {
            info.ReplyLocalized(Localizer, "NotEnoughArgs", 1, info.ArgCount);
            return;
        }

        var mapName = info.GetArg(1);

        if (!Config.Maps.Any(x => x.Name == mapName))
        {
            info.ReplyLocalized(Localizer, "NotExistingMap");
            return;
        }

        Config.RemoveMap(caller, mapName);
        info.ReplyLocalized(Localizer, "MapRemoved", mapName);
    }


    [ConsoleCommand("nextmap", "Gets/sets the next map of the cycle")]
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
            info.ReplyLocalized(Localizer, "NextMapNow", _nextMap.DName());
        }
    }

    [ConsoleCommand("go", "Direct switch to the map you want of the cycle")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper( whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnGoToNextMapCommand(CCSPlayerController? caller, CommandInfo info)
    {
        ChangeMap();
    }

    [ConsoleCommand("goto", "Direct switch to the next map of the cycle")]
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
            info.ReplyLocalized(Localizer, "NextMap", _nextMap.DName());
        }
    }

    private void AutoMapCycle()
    {
        if (_rtv != null && _rtv.NextMap != null)
        {
            _nextMap = _rtv.NextMap;
        }
        // Print the next map
        LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMap", _nextMap.DName());
        // Change the map
        AddTimer(19f, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
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
