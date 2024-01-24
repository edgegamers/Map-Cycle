using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace MapCycle
{
    public partial class ConfigGen : BasePluginConfig
    {

        [JsonPropertyName("ConfigVersion")]
        public override int Version { get; set; } = 2;

        private string _fileName = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/MapCycle/MapCycle.json";

        public void RewriteConfig()
        {
            var config = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_fileName, config);
        }

        public void Reload()
        {
            var config = File.ReadAllText(_fileName);

            var settings = new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            };

            var jsonObject = JObject.Parse(config, settings);
            var newConfig = jsonObject.ToObject<ConfigGen>();

            if (newConfig != null)
            {
                Maps = newConfig.Maps;
                MapCycle = newConfig.MapCycle;
                Rtv = newConfig.Rtv;
            }
        }

        public string GetConfigValue(string optionName)
        {
            if (optionName == null) return string.Empty;

            var config = File.ReadAllText(_fileName);

            var settings = new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            };

            var jsonObject = JObject.Parse(config, settings);
            var value = jsonObject[optionName]?.ToString() ?? string.Empty;

            return value;
        }

    }
}