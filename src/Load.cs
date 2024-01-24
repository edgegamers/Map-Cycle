using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;

namespace MapCycle
{
    public partial class MapCycle
    {
        public override void Load(bool hotReload)
        {

            if(Config.Version < 2)
            {
                AddTimer(20, AlertOfBadConfigVersionRecursive, TimerFlags.STOP_ON_MAPCHANGE);
                return;
            }

            // change the convar mp_match_end_changelevel to 0
            var chLvlCvar = ConVar.Find("mp_match_end_changelevel");
            chLvlCvar?.SetValue(false);

            if (Config.Rtv.Enabled)
                InitRTV();
            else
                SetNextMap(Server.MapName);

            InitEvents();
        }

        public void AlertOfBadConfigVersionRecursive()
        {
            Server.PrintToConsole("[MapCycle ERROR] Bad config version. Save your maps in a separate file, delete the config file and restart the server.");
            Server.PrintToChatAll($" {ChatColors.Red}[MapCycle ERROR] {ChatColors.Default}Bad config version. Save your maps in a separate file, delete the config file and restart the server.");
            AddTimer(60, AlertOfBadConfigVersionRecursive, TimerFlags.STOP_ON_MAPCHANGE);
        }
    }
}