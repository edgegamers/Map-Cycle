using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapCycle
{
    public partial class ConfigGen : BasePluginConfig
    {

        [JsonPropertyName("MapCycle")]
        public MapCycleConfig MapCycle { get; set; } = new MapCycleConfig();
    }
}

public class MapCycleConfig
{
    // Rtv enabled
    [JsonPropertyName("RandomOrder")]
    public bool Randomize { get; set; } = false;
}