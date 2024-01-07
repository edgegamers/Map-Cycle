using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Core.Translations;

namespace MapCycle;

public static class LocalizationExtension
{
    public static void ReplyLocalized(this CommandInfo info, IStringLocalizer localizer, string key, params object[] args)
    {
        string value = localizer[key, args];
        value = value.Replace("%prefix%", localizer["prefix"]);
        info.ReplyToCommand(value);
    }

    public static void PrintLocalizedChatAll(IStringLocalizer localizer, string key, params object[] args)
    {
        string value = localizer[key, args];
        value = value.Replace("%prefix%", localizer["prefix"]);
        Server.PrintToChatAll(value);
    }
}