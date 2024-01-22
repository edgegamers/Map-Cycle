using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace MapCycle
{
    public partial class ConfigGen : BasePluginConfig
    {
        [JsonPropertyName("Rtv")]
        public RtvConfig Rtv { get; set; } = new RtvConfig();
    }
}

public class RtvConfig
{
    // Rtv enabled
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;


    // Automatic map vote
    [JsonPropertyName("AutoVoteEnabled")]
    public bool AutoVoteEnabled { get; set; } = false;

    [JsonPropertyName("AutoVoteRoundStart")]
    public int AutoVoteRoundStart { get; set; } = 3;

    [JsonPropertyName("AutoVoteStartAtTheEndOfMatch")]
    public bool AutoVoteStartAtTheEndOfMatch { get; set; } = true;


    // Vote triggered by player
    [JsonPropertyName("PlayerCommandEnabled")]
    public bool PlayerCommandEnabled { get; set; } = false;

    [JsonPropertyName("PlayerCommandChangeTheMapDirectlyAfterVote")]
    public bool PlayerCommandChangeTheMapDirectlyAfterVote { get; set; } = false;


    // Vote settings
    [JsonPropertyName("VoteMapCount")]
    public int VoteMapCount { get; set; } = 5;

    [JsonPropertyName("VoteDurationInSeconds")]
    public int VoteDurationInSeconds { get; set; } = 30;

    [JsonPropertyName("VoteRatioEnabled")]
    public bool VoteRatioEnabled { get; set; } = true;

    [JsonPropertyName("VoteRatio")]
    public float VoteRatio { get; set; } = 0.5f;
}