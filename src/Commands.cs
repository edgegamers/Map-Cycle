using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;

namespace MapCycle
{
    public partial class MapCycle
    {
        // ----------------- Admin Commands ----------------- //

        [ConsoleCommand("addmap", "Add a new map in the cycle")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(minArgs: 3, usage: "<#map name> <#display name> <#id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnAddMap(CCSPlayerController? caller, CommandInfo info)
        {
            if (info.ArgCount < 3)
            {
                info.ReplyLocalized(Localizer, "NotEnoughArgs", 3, info.ArgCount);
                return;
            }

            var mapName = info.GetArg(1);
            var displayName = info.GetArg(2);
            var id = info.GetArg(3);
            bool workshop = true;

            if (id == mapName)
                workshop = false;

            Config.AddMap(mapName, displayName, id, workshop, info, Localizer);
        }

        [ConsoleCommand("go", "Direct switch to the map you want of the cycle")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
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

            // if the map is a workshop id then we use the host_workshop_map command
            var isNumber = Regex.IsMatch(commandMapName, @"^\d+$");

            if (isNumber)
            {
                _lastVisitedMap = commandMapName;
                Server.ExecuteCommand($"host_workshop_map {commandMapName}");
                return;
            }

            var map = Config.Maps.FirstOrDefault(x => x.Name == commandMapName);
            if (map == null)
            {
                _lastVisitedMap = commandMapName;
                Server.ExecuteCommand($"map {commandMapName}");
                return;
            }
            else
            {
                // Else we change the map
                _nextMap = map;
                ChangeMap();
            }
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

            if(caller != null)
                Config.RemoveMap(caller, mapName, info, Localizer);
        }

        [ConsoleCommand("keepmap", "Keep the current map in the cycle")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(minArgs: 0, usage: "<#(optional)map display name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnKeepMapCommand(CCSPlayerController? caller, CommandInfo info)
        {
            var currentMapName = Server.MapName;
            var lastVisitedMap = _lastVisitedMap;
            var displayName = info.GetArg(1);
            var workshop = currentMapName != lastVisitedMap;

            if (string.IsNullOrEmpty(displayName))
                displayName = currentMapName;

            if(lastVisitedMap == null) return;

            Config.AddMap(currentMapName, displayName, lastVisitedMap, workshop, info, Localizer);
        }

        [ConsoleCommand("resetrtv", "Keep the current map in the cycle")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(minArgs: 0, usage: "<#(optional)map display name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnResetRtvCommand(CCSPlayerController? caller, CommandInfo info)
        {
            if (_rtv == null) return;
            
            _rtv.Reset();
            info.ReplyLocalized(Localizer, "RtvReset");
        }

        [ConsoleCommand("cfgr", "Reload the config in the current session without restarting the server")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnReloadCommand(CCSPlayerController? caller, CommandInfo info)
        {
            Config.Reload();
            info.ReplyLocalized(Localizer, "ConfigReloaded");
        }

        [ConsoleCommand("cfgc", "Check the config")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(minArgs: 1, usage: "<# ConfigOptionName e.g MapCycle>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnCheckConfigCommand(CCSPlayerController? caller, CommandInfo info)
        {
            if (Config == null) return;
            if (Localizer == null) return;

            var configName = info.GetArg(1);
            // fetch confignamme value
            var configValue = Config.GetConfigValue(configName);
            Server.PrintToChatAll($"MapCycle config: {configValue}");
        }


        // ----------------- Player + Admin Commands ----------------- //

        [ConsoleCommand("nextmap", "Gets/sets the next map of the cycle")]
        public void OnSetNextCommand(CCSPlayerController? caller, CommandInfo info)
        {
            // admin part
            if (info.ArgCount == 1 || !AdminManager.PlayerHasPermissions(caller, "@css/changemap"))
            {
                if (_nextMap == null)
                {
                    info.ReplyLocalized(Localizer, "NextMapUnset");
                }
                else
                {
                    info.ReplyLocalized(Localizer, "NextMap", _nextMap.DName());
                }
                return;
            }

            // player part
            var commandMapName = info.GetArg(1);
            var map = Config.Maps.FirstOrDefault(x => x.Name == commandMapName);
            if (map == null)
            {
                info.ReplyLocalized(Localizer, "NotExistingMap", commandMapName);
                return;
            }
            else
            {
                _nextMap = map;
                info.ReplyLocalized(Localizer, "NextMapNow", _nextMap.DName());
            }
        }

        [ConsoleCommand("rtv", "Start a map vote")]
        public void OnRtvCommand(CCSPlayerController? caller, CommandInfo info)
        {
            if(!Config.Rtv.PlayerCommandEnabled){
                info.ReplyLocalized(Localizer, "RtvCommandDisabled");
                return;
            }
            if (_rtv == null) return;

            if(_rtv.PlayerSaidRtv.Contains(caller!.PlayerName)){
                LocalizationExtension.PrintLocalizedChat(caller, Localizer, "AlreadySaidRtv");
                return;
            }

            _rtv.PlayerSaidRtv.Add(caller!.PlayerName);
            var PlayerSaidRtvCount = _rtv.PlayerSaidRtv.Count();
            var playerWithoutBotsCountFloat = (float)Utilities.GetPlayers().Count(p => !p.IsBot);
            var minimumPlayerCount = (int)playerWithoutBotsCountFloat * Config.Rtv.PlayerCommandRatio;
            var hasEnoughRtv = PlayerSaidRtvCount >= minimumPlayerCount;

            if (Config.Rtv.PlayerCommandTriggerAVote){
                if(Config.Rtv.PlayerCommandRatioEnabled){
                    if(hasEnoughRtv)
                        _rtv.Call(Config.Rtv.VoteDurationInSeconds, true);
                    else
                        LocalizationExtension.PrintLocalizedChatAll(Localizer, "RtvNeedMoreRtvForVote", PlayerSaidRtvCount, minimumPlayerCount);
                } else {
                    _rtv.Call(Config.Rtv.VoteDurationInSeconds, true);
                }
            } else {
                if (Config.Rtv.PlayerCommandRatioEnabled)
                {
                    if (hasEnoughRtv)
                        ChangeMapWithAnnounce();
                    else
                        LocalizationExtension.PrintLocalizedChatAll(Localizer, "RtvNeedMoreRtvForSwitchMap", PlayerSaidRtvCount, minimumPlayerCount);
                } else {
                    ChangeMapWithAnnounce();
                }
            }
        }
    }
}