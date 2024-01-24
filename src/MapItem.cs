using CounterStrikeSharp.API;

namespace MapCycle
{
    public class MapItem
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Id { get; set; }
        public bool Workshop { get; set; }

        public string DName()
        {
            if(DisplayName != null && DisplayName != "")
                return DisplayName;
            else
                return Name;
        }

        public void Start()
        {
            if(Workshop)
                Server.ExecuteCommand($"host_workshop_map {Id}");
            else
                Server.ExecuteCommand($"map {Name}");
        }
    }
}