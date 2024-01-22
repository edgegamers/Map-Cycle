using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;

namespace MapCycle
{
    public partial class ConfigGen : BasePluginConfig
    {
        private string _fileName = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/MapCycle/MapCycle.json";

        public void RewriteConfig()
        {
            var config = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_fileName, config);
        }

    }
}