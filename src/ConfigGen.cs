using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapCycle
{
    public class ConfigGen : BasePluginConfig
    {
        [JsonPropertyName("Maps")]
        public List<MapItem> Maps { get; set; } = new List<MapItem>
        {
            new MapItem { Name = "de_dust2", DisplayName = "Dust 2", Id = "de_dust2", Workshop = false },
            new MapItem { Name = "de_aztec", DisplayName = "Aztec", Id = "3070960099", Workshop = true }
        };

        [JsonPropertyName("Randomize")]
        public bool Randomize { get; set; } = false;

        [JsonPropertyName("RtvEnabled")]
        public bool RtvEnabled { get; set; } = false;

        [JsonPropertyName("RtvPlayerCommandEnabled")]
        public bool RtvPlayerCommandEnabled { get; set; } = false;

        [JsonPropertyName("RtvPlayerCommandChangeTheMapDirectlyAfterVote")]
        public bool RtvPlayerCommandChangeTheMapDirectlyAfterVote { get; set; } = false;

        [JsonPropertyName("RtvMapCount")]
        public int RtvMapCount { get; set; } = 5;

        [JsonPropertyName("RtvRoundStartVote")]
        public int RtvRoundStartVote { get; set; } = 3;

        [JsonPropertyName("RtvStartVoteAtTheEnd")]
        public bool RtvStartVoteAtTheEnd { get; set; } = true;

        [JsonPropertyName("RtvDurationInSeconds")]
        public int RtvDurationInSeconds { get; set; } = 30;

        [JsonPropertyName("RtvVoteRatioEnabled")]
        public bool RtvVoteRatioEnabled { get; set; } = true;

        [JsonPropertyName("RtvVoteRatio")]
        public float RtvVoteRatio { get; set; } = 0.5f;

        private string _fileName = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/MapCycle/MapCycle.json";

        public void RewriteConfig()
        {
            var config = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_fileName, config);
        }

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
    }
}