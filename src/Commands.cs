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

            if (Config.Maps.Any(x => x.Name == mapName))
            {
                info.ReplyLocalized(Localizer, "AlreadyExistingMap", mapName);
                return;
            }

            if (id == mapName)
            {
                workshop = false;
            }

            Config.AddMap(mapName, displayName, id, workshop);
            info.ReplyLocalized(Localizer, "MapAdded", mapName);
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

            if (!Config.Maps.Any(x => x.Name == mapName))
            {
                info.ReplyLocalized(Localizer, "NotExistingMap", mapName);
                return;
            }

            if(caller != null)
            {
                Config.RemoveMap(caller, mapName);
            }
            info.ReplyLocalized(Localizer, "MapRemoved", mapName);
        }

        [ConsoleCommand("keepmap", "Keep the current map in the cycle")]
        [RequiresPermissions("@css/changemap")]
        [CommandHelper(minArgs: 0, usage: "<#(optional)map display name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnKeepMapCommand(CCSPlayerController? caller, CommandInfo info)
        {
            var currentMapName = Server.MapName;
            var lastVisitedMap = _lastVisitedMap;
            var displayName = info.GetArg(1);
            var workshop = false;

            _lastVisitedMap = null;

            if (currentMapName != lastVisitedMap)
            {
                workshop = true;
            }

            if (displayName == null || displayName == "")
            {
                displayName = currentMapName;
            }

            if(lastVisitedMap == null) return;

            if (Config.Maps.Any(x => x.Name == lastVisitedMap) || Config.Maps.Any(x => x.Id == lastVisitedMap))
            {
                info.ReplyLocalized(Localizer, "AlreadyExistingMap", lastVisitedMap);
                return;
            }

            Config.AddMap(currentMapName, displayName, lastVisitedMap, workshop);
            info.ReplyLocalized(Localizer, "MapAdded", currentMapName);
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
            if(!Config.RtvPlayerCommandEnabled){
                info.ReplyLocalized(Localizer, "RtvCommandDisabled");
                return;
            }

            if(_rtv == null) return;
            _rtv.Call(Config.RtvDurationInSeconds, true);
        }
    }
}