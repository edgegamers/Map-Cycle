using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace MapCycle
{ 
    public partial class ConfigGen : BasePluginConfig
    {
        [JsonPropertyName("Maps")]
        public List<MapItem> Maps { get; set; } = new List<MapItem>
        {
            new MapItem { Name = "de_dust2", DisplayName = "Dust 2", Id = "de_dust2", Workshop = false },
            new MapItem { Name = "de_aztec", DisplayName = "Aztec", Id = "3070960099", Workshop = true }
        };


        public void AddMap(string mapName, string displayName, string id, bool workshop)
        {
            Maps.Add(new MapItem { Name = mapName, DisplayName = displayName, Id = id, Workshop = workshop });
            RewriteConfig();
        }

        public void RemoveMap(CCSPlayerController player, string mapName)
        {
            Maps.RemoveAll(x => x.Name == mapName);
            RewriteConfig();
        }

        public void ChangeMapDisplayName(CCSPlayerController player, string mapName, string displayName)
        {
            var map = Maps.FirstOrDefault(x => x.Name == mapName);
            if (map != null)
            {
                map.DisplayName = displayName;
                RewriteConfig();
            }
        }
    }
}