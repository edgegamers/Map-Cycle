
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
            {
                return DisplayName;
            } else {
                return Name;
            }
        }
    }
}