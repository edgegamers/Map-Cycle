
namespace MapCycle
{
    public class MapItem
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
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