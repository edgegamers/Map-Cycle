using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;

using CounterStrikeSharp.API.Modules.Timers;


namespace MapCycle;

[MinimumApiVersion(80)]

// Main class of the plugin
public partial class MapCycle : BasePlugin, IPluginConfig<ConfigGen>
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
    protected string? _lastVisitedMap = null;

    public void InitRTV()
    {
        if(_rtv == null){
            _rtv = Rtv.Instance;
            _rtv.Config = Config;
            _rtv.Localizer = Localizer;
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
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}************ERROR MAP CYCLE*************");
                Server.PrintToChatAll($" [MapCycle] The current map doesn't exist in the map cycle: {Server.MapName}");
                Server.PrintToChatAll($" [MapCycle] Please check that the map is correcly named in the json config.");
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}************ERROR MAP CYCLE*************");
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
