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
    public override string ModuleVersion => "1.4.1";

    // plugin configs
    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) { Config = config; }

    // private variables
    private MapItem? _nextMap;

    private MapItem? _currentMap;
    private Random _randomIndex = new Random();
    private Rtv? _rtv;

    // Protected variables
    protected string? _lastVisitedMap = null;

    public void InitRTV()
    {
        // Initialize the rtv instance if it is null
        _rtv ??= Rtv.Instance;
        _rtv.Reset();
        _rtv.Config = Config;
        _rtv.Localizer = Localizer;

        // Initialize the rtv end vote events
        _rtv.EndVoteEvent += OnRtvEndVoteEvent;
    }

    private void OnRtvEndVoteEvent(object sender, EventArgs e)
    {
        _nextMap = _rtv?.NextMap;

        if (_nextMap == null)
            SetNextMap(Server.MapName);
        if (_nextMap == null || _rtv == null) return;

        LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMapNow", _nextMap.DName());

        if (Config.Rtv.PlayerCommandChangeTheMapDirectlyAfterVote && _rtv.PlayerVoteEnded)
            ChangeMapWithAnnounce();
    }

    private void CountDownChangeMapMessage()
    {
        AddTimer(1, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 3), TimerFlags.STOP_ON_MAPCHANGE);
        AddTimer(2, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 2), TimerFlags.STOP_ON_MAPCHANGE);
        AddTimer(3, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 1), TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void ChangeMapWithAnnounce()
    {
        CountDownChangeMapMessage();
        AddTimer(4, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void AutoMapCycle()
    {
        if (_rtv != null && _rtv.NextMap != null)
            _nextMap = _rtv.NextMap;

        if (_nextMap == null)
            SetNextMap(Server.MapName);

        // Change the map
        AddTimer(19f, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeMap()
    {
        _rtv?.Reset();
        if (_nextMap == null) return;

        _nextMap.Start();
    } 

    private void SetNextMap(string mapName)
    {
        _nextMap = Config.Maps[GetNextMapIndex() % Config.Maps.Count];
    }

    private void TriggerAutoMapVoteByTime(string mapName)
    {
        if (Config.Rtv.AutoVoteTimeStartInSeconds <= 0) return;

        AddTimer(Config.Rtv.AutoVoteTimeStartInSeconds, () =>
        {
            if (_rtv == null) return;
            _rtv.Call(Config.Rtv.VoteDurationInSeconds);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private int GetNextMapIndex()
    {
        var nextIndex = CurrentMapIndex() + 1;

        if (Config.MapCycle.Randomize)
            nextIndex = GetRandomMapIndexExcludingCurrent();

        return nextIndex;
    }

    private int GetRandomMapIndexExcludingCurrent()
    {
        int nextIndex;
        do
        {
            nextIndex = _randomIndex.Next(0, Config.Maps.Count);
        } while (nextIndex == CurrentMapIndex());

        return nextIndex;
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

    private MapItem? CurrentMap()
    {
        _currentMap = Config.Maps.FirstOrDefault(x => x.Name == Server.MapName);
        if (_currentMap == null)
            ErrorCurrentMapMessage();

        return _currentMap;
    }

    private void ErrorCurrentMapMessage()
    {
        Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}************ERROR MAP CYCLE*************");
        Server.PrintToChatAll($" [MapCycle] The current map doesn't exist in the map cycle: {Server.MapName}");
        Server.PrintToChatAll($" [MapCycle] Please check that the map is correcly named in the json config.");
        Server.PrintToChatAll($" [MapCycle] Or type {ChatColors.LightRed}!keepmap {ChatColors.Default}to add it to the map cycle");
        Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}************ERROR MAP CYCLE*************");
    }
}
