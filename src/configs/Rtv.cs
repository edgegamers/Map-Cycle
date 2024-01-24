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
    public bool Enabled { get; set; } = true;


    // Automatic map vote
    [JsonPropertyName("AutoVoteEnabled")]
    public bool AutoVoteEnabled { get; set; } = true;

    [JsonPropertyName("AutoVoteRoundStart")]
    public int AutoVoteRoundStart { get; set; } = 3;

    [JsonPropertyName("AutoVoteStartAtTheEndOfMatch")]
    public bool AutoVoteStartAtTheEndOfMatch { get; set; } = false;


    // Vote triggered by player
    [JsonPropertyName("PlayerCommandEnabled")]
    public bool PlayerCommandEnabled { get; set; } = true;
    
    [JsonPropertyName("PlayerCommandTriggerAVote")]
    public bool PlayerCommandTriggerAVote { get; set; } = false;

    [JsonPropertyName("PlayerCommandRatioEnabled")]
    public bool PlayerCommandRatioEnabled { get; set; } = false;

    [JsonPropertyName("PlayerCommandRatio")]
    public float PlayerCommandRatio { get; set; } = 0.5f;

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