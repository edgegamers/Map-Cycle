using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace MapCycle
{
    public class ConfigGen : BasePluginConfig
    {
        [JsonPropertyName("Maps")]
        public List<MapItem> Maps { get; set; } = new List<MapItem>
        {
            new MapItem { Name = "de_dust2", Id = "de_dust2", Workshop = false },
            new MapItem { Name = "de_aztec", Id = "3070960099", Workshop = true }
        };

        [JsonPropertyName("Randomize")]
        public bool Randomize { get; set; } = false;

        [JsonPropertyName("RtvEnabled")]
        public bool RtvEnabled { get; set; } = false;

        [JsonPropertyName("RtvMapCount")]
        public int RtvMapCount { get; set; } = 5;

        [JsonPropertyName("RtvRoundStartVote")]
        public int RtvRoundStartVote { get; set; } = 3;

        [JsonPropertyName("RtvStartVoteAtTheEnd")]
        public bool RtvStartVoteAtTheEnd { get; set; } = true;

        [JsonPropertyName("RtvDurationInSeconds")]
        public int RtvDurationInSeconds { get; set; } = 30;

        [JsonPropertyName("RtvVoteRatio")]
        public float RtvVoteRatio { get; set; } = 0.5f;
    }
}