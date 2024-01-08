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
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using System.Text.RegularExpressions;
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
        public IStringLocalizer Localizer { get; set; }

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

        public void Call(int duration)
        {
            SetRandomMapList();
            StartVote(duration);
        }

        public void StartVote(int duration)
        {
            VoteEnabled = true;
            VoteCount = 0;
            VoteList.Clear();
            PlayerVotedList.Clear();
            RtvCommand();
            AddTimer(duration, EndVote, TimerFlags.STOP_ON_MAPCHANGE);
        }

        public void EndVote()
        {
            VoteEnabled = false;
            int mapIndex = -1;
            if (VoteList.Count != 0) {
                mapIndex = VoteList.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
            }
            if(mapIndex == -1) {
                LocalizationExtension.PrintLocalizedChatAll(Localizer, "NoVotes");
                OnEndVote(EventArgs.Empty);
                return;
            }

            NextMap = MapList[mapIndex];
            LocalizationExtension.PrintLocalizedChatAll(Localizer, "VoteFinished");
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

            var menu = new ChatMenu(Localizer["AnnounceVoteHow"]);
            var i = 1;
            MapList.ForEach(map => {
                menu.AddMenuOption(Localizer["VoteRankFormat", i, map.Name], (controller, options) => {
                    AddVote(controller, options);
                });
                i++;
            });

            foreach (var player in Utilities.GetPlayers())
            {
                ChatMenus.OpenMenu(player, menu);
            }
        }

        public void AddVote(CCSPlayerController? caller, ChatMenuOption info)
        {
            string pattern = @"\[([0-9]+)\]";
            Match match = Regex.Match(info.Text, pattern);
            try
            {
                if(PlayerVotedList.Contains(caller!.PlayerName))
                {
                    LocalizationExtension.PrintLocalizedChatAll(Localizer, "AlreadyVoted");
                    return;
                } else {
                    int number = int.Parse(match.Groups[1].Value);
                    var commandIndex = number - 1;
                    if(commandIndex > MapList.Count - 1 || commandIndex < 0)
                    {
                        Server.PrintToChatAll($"Player {caller!.PlayerName} voted for {commandIndex}");
                        LocalizationExtension.PrintLocalizedChatAll(Localizer, "VoteInvalid");
                        return;
                    } else {
                        PlayerVotedList.Add(caller!.PlayerName);
                        VoteList.Add(commandIndex);
                        VoteCount++;
                        LocalizationExtension.PrintLocalizedChatAll(Localizer, "VoteConfirm", MapList[commandIndex].Name);
                    }
                }
                
            }
            catch (Exception e)
            {
                Server.PrintToConsole($" {ChatColors.Red}[MapCycleError] {ChatColors.Default}{e}");
                LocalizationExtension.PrintLocalizedChatAll(Localizer, "VoteInvalid");
            }
        }
    }
}