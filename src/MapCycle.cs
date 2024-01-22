using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;

using CounterStrikeSharp.API.Modules.Timers;


namespace MapCycle;

[MinimumApiVersion(80)]

// Main class of the plugin
public partial class MapCycle : BasePlugin, IPluginConfig<ConfigGen>
{
    // plugin informations
    public override string ModuleName => "MapCycle";
    public override string ModuleAuthor => "NANOR";
    public override string ModuleVersion => "1.3.6";

    // plugin configs
    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) { Config = config; }

    // private variables
    private MapItem? _nextMap;
    CCSGameRules gamerules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    private MapItem? _currentMap;
    private Random _randomIndex = new Random();
    private Rtv? _rtv;

    // Protected variables
    protected string? _lastVisitedMap = null;

    public void InitRTV()
    {
        // Init the rtv instance
        if(_rtv == null){
            _rtv = Rtv.Instance;
            _rtv.Config = Config;
            _rtv.Localizer = Localizer;
        }

        // Init the rtv end vote events
        _rtv.EndVoteEvent += (sender, e) =>
        {
            _nextMap = _rtv.NextMap;

            if (_nextMap == null)
                SetNextMap(Server.MapName);
            if (_nextMap == null) return;

            LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMapNow", _nextMap.DName());

            if (Config.Rtv.PlayerCommandChangeTheMapDirectlyAfterVote && _rtv.PlayerVoteEnded)
            {
                CountDownChangeMapMessage();
                AddTimer(4, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
            }
        };
    }

    private void CountDownChangeMapMessage()
    {
        AddTimer(1, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 3), TimerFlags.STOP_ON_MAPCHANGE);
        AddTimer(2, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 2), TimerFlags.STOP_ON_MAPCHANGE);
        AddTimer(3, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 1), TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void AutoMapCycle()
    {
        if (_rtv != null && _rtv.NextMap != null)
            _nextMap = _rtv.NextMap;

        if (_nextMap == null)
            SetNextMap(Server.MapName);

        // Print the next map if it is not null
        if (_nextMap != null)
            LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMap", _nextMap.DName());
        
        // Change the map
        AddTimer(19f, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeMap()
    {
        if(_rtv != null)
            _rtv.NextMap = null;
        
        if (_nextMap == null) return;

        // If the next map is a workshop map, we use the host_workshop_map command
        if (_nextMap.Workshop)
            Server.ExecuteCommand($"host_workshop_map {_nextMap.Id}");
        else
            Server.ExecuteCommand($"map {_nextMap.Name}");
    } 

    private void SetNextMap(string mapName)
    {
        if (_rtv != null)
        {
            // reset rtv variables
            _rtv.alreadyVotedByPlayer = false;
            _rtv.VoteEnabled = false;
            _rtv.VoteCount = 0;
            _rtv.VoteList.Clear();
            _rtv.PlayerVotedList.Clear();
            _rtv.NextMap = null;
        }

        // By default, we set the first map of the cycle
        _nextMap = Config.Maps[0];
        // Get the next map index
        var _nextIndex = CurrentMapIndex() + 1;

        // If the randomize option is enabled, we set a random map
        if (Config.MapCycle.Randomize)
        {
            do {
                _nextIndex = _randomIndex.Next(0, Config.Maps.Count);
            } while (_nextIndex == CurrentMapIndex());
        }

        // If the next map index is greater than the map cycle count, we let the first map of the cycle
        if (_nextIndex < Config.Maps.Count)
            _nextMap = Config.Maps[_nextIndex];
        
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
                ErrorCurrentMapMessage();

            return _currentMap;
        }
    }

    private void ErrorCurrentMapMessage()
    {
        Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}************ERROR MAP CYCLE*************");
        Server.PrintToChatAll($" [MapCycle] The current map doesn't exist in the map cycle: {Server.MapName}");
        Server.PrintToChatAll($" [MapCycle] Please check that the map is correcly named in the json config.");
        Server.PrintToChatAll($" [MapCycle] Or type {ChatColors.LightRed}!keepmap {ChatColors.Default}to add it to the map cycle");
        Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}************ERROR MAP CYCLE*************");
    }

    private int CurrentMapIndex()
    {
        var currentMap = CurrentMap();
        if(currentMap == null) {
            return 0;
        } else {
            return Config.Maps.FindIndex(a => a.Name == currentMap.Name);
        }
    }
}
