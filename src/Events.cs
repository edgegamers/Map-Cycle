using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace MapCycle
{
    public partial class MapCycle
    {
        private void InitEvents()
        {
            RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
                int roundsPlayed = gameRules.TotalRoundsPlayed + 1;
                if (_rtv == null) return HookResult.Continue;

                if (Config.Rtv.Enabled && roundsPlayed == Config.Rtv.AutoVoteRoundStart && !Config.Rtv.AutoVoteStartAtTheEndOfMatch) // +1 for the warmup
                {
                    _rtv.Call(Config!.Rtv.VoteDurationInSeconds);
                }
                return HookResult.Continue;
            });

            // Create the timer to change the map
            RegisterEventHandler<EventCsWinPanelMatch>((@event, info) =>
            {
                if (_rtv == null) return HookResult.Continue;
                // Start the vote at the end of the match
                if (Config.Rtv.AutoVoteStartAtTheEndOfMatch && Config.Rtv.Enabled)
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
