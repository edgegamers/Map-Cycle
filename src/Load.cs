using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Cvars;

namespace MapCycle
{
    public partial class MapCycle
    {
        public override void Load(bool hotReload)
        {
            // change the convar mp_match_end_changelevel to 0
            var chLvlCvar = ConVar.Find("mp_match_end_changelevel");
            chLvlCvar?.SetValue(false);

            if (!Config.RtvEnabled)
            {
                SetNextMap(Server.MapName);
            }
            else
            {
                InitRTV();
                if (_rtv == null) return;

                _rtv.EndVoteEvent += (sender, e) =>
                {
                    _currentRound = -100;

                    _nextMap = _rtv.NextMap;
                    if(_nextMap == null) {
                        SetNextMap(Server.MapName);
                    }

                    if (_nextMap == null) return;

                    LocalizationExtension.PrintLocalizedChatAll(Localizer, "NextMapNow", _nextMap.DName());

                    if (Config.RtvPlayerCommandChangeTheMapDirectlyAfterVote && _rtv.PlayerVoteEnded)
                    {
                        AddTimer(1, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 3), TimerFlags.STOP_ON_MAPCHANGE);
                        AddTimer(2, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 2), TimerFlags.STOP_ON_MAPCHANGE);
                        AddTimer(3, () => LocalizationExtension.PrintLocalizedChatAll(Localizer, "MapChangingIn", 1), TimerFlags.STOP_ON_MAPCHANGE);
                        AddTimer(4, ChangeMap, TimerFlags.STOP_ON_MAPCHANGE);
                    }
                };
            }

            RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                _currentRound++;
                if (_rtv == null) return HookResult.Continue;

                if (Config.RtvEnabled && _currentRound == Config.RtvRoundStartVote + 1 && !Config.RtvStartVoteAtTheEnd) // +1 for the warmup
                {
                    _rtv.Call(Config!.RtvDurationInSeconds);
                }
                return HookResult.Continue;
            });

            // Create the timer to change the map
            RegisterEventHandler<EventCsWinPanelMatch>((@event, info) =>
            {
                if (_rtv == null) return HookResult.Continue;
                // Start the vote at the end of the match
                if (Config.RtvStartVoteAtTheEnd && Config.RtvEnabled)
                {
                    _rtv.Call(15);
                }
                AutoMapCycle();
                return HookResult.Continue;
            });

            RegisterListener<Listeners.OnMapStart>(SetNextMap);

        }
    }
}