using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using NsJsonSerializer = Newtonsoft.Json.JsonSerializer;
using Newtonsoft.Json.Linq;

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

    }
}