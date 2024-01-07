using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using System.Linq;

namespace MapCycle
{
    internal class Rtv : BasePlugin
    {
        public int VoteCount = 0;
        public bool VoteEnabled = true;
        public List<int> VoteList = new List<int>();
        public List<MapItem> MapList = new List<MapItem>();
        public List<string> PlayerVotedList = new List<string>();
        public MapItem? NextMap;
        public ConfigGen? Config { get; set; }

        public override string ModuleName => throw new NotImplementedException();

        public override string ModuleVersion => throw new NotImplementedException();


        // Singleton Instance ---------------------------------------
        private static Rtv _instance;

        public static Rtv Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Rtv();
                }
                return _instance;
            }
        }

        // Constructor ---------------------------------------

        public Rtv()
        {
        }

        // End vote Event ---------------------------------------
        public delegate void EndVoteEventHandler(object sender, EventArgs e);

        // Declare event
        public event EndVoteEventHandler EndVoteEvent;

        // Trigger event
        protected virtual void OnEndVote(EventArgs e)
        {
            EndVoteEvent?.Invoke(this, e);
        }

        public void Call()
        {
            SetRandomMapList();
            StartVote();
        }

        public void StartVote()
        {
            VoteEnabled = true;
            VoteCount = 0;
            VoteList.Clear();
            PlayerVotedList.Clear();
            RtvCommand();
            AddTimer(Config.RtvDurationInSeconds, EndVote, TimerFlags.STOP_ON_MAPCHANGE);
        }

        public void EndVote()
        {
            VoteEnabled = false;
            int mapIndex = -1;
            if (VoteList.Count != 0) {
                mapIndex = VoteList.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
            }
            if(mapIndex == -1) {
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}No one voted, the next map will be chosen in the map cycle");
                OnEndVote(EventArgs.Empty);
                return;
            }

            NextMap = MapList[mapIndex];
            Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}The vote is finished!");
            OnEndVote(EventArgs.Empty);
            VoteList = new List<int>();
            PlayerVotedList = new List<string>();
        }

        public void SetRandomMapList()
        {
            Random rnd = new Random();
            List<MapItem> configList = Config.Maps;
            List<MapItem> shuffledList = configList.OrderBy(x => rnd.Next()).ToList();

            List<MapItem> randomElements = shuffledList.Take(Config.RtvMapCount).ToList();
            MapList = randomElements;
        }

        public void RtvCommand()
        {
            Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}Vote for the next map by typing: !css_vote number");
        
            var i = 1;
            MapList.ForEach(map => {
                Server.PrintToChatAll($" {ChatColors.Red}[MapCycle] {ChatColors.Default}[{ChatColors.Green}{i}{ChatColors.Default}] - {map.Name}");
                i++;
            });
        }

        public void AddVote(CCSPlayerController? caller, CommandInfo info)
        {
            try
            {
                if(!VoteEnabled)
                {
                    info.ReplyToCommand($" {ChatColors.Red}[MapCycle] {ChatColors.Default}Votes are not yet open");
                    return;
                }

                if(PlayerVotedList.Contains(caller!.PlayerName))
                {
                    info.ReplyToCommand($" {ChatColors.Red}[MapCycle] {ChatColors.Default}You already voted");
                    return;
                } else {
                    var commandIndex = int.Parse(info.GetArg(1)) - 1;
                    if(commandIndex > MapList.Count - 1 || commandIndex < 0)
                    {
                        info.ReplyToCommand($" {ChatColors.Red}[MapCycle] {ChatColors.Default}Vote invalid");
                        return;
                    } else {
                        PlayerVotedList.Add(caller!.PlayerName);
                        VoteList.Add(commandIndex);
                        VoteCount++;
                        info.ReplyToCommand($" {ChatColors.Red}[MapCycle] {ChatColors.Default}You voted for {MapList[commandIndex].Name}");
                    }
                }
                
            }
            catch (Exception e)
            {
                Server.PrintToConsole($" {ChatColors.Red}[MapCycleError] {ChatColors.Default}{e}");
                info.ReplyToCommand($" {ChatColors.Red}[MapCycle] {ChatColors.Default}Vote invalid");
            }
        }
    }
}