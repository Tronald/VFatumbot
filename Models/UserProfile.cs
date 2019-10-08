using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public string Name { get; set; }
        public double appi = 1000;

        // Fukuoka shrine where I got married
        public double tmplat = 33.667612;
        public double tmplon = 130.3131111;

        public int tmprad = 3000;
        public bool includeWater = false;

        public LatLng Location
        {
            get
            {
                return new LatLng(tmplat, tmplon);
            }
        }
    }
}
