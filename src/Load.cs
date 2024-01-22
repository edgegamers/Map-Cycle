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

            if (Config.Rtv.Enabled)
            {
                InitRTV();
            }
            else
            {
                SetNextMap(Server.MapName);
            }
            InitEvents();
        }
    }
}