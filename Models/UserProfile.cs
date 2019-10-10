using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public double Latitude = 200d;
        public double Longitude = 200d;
        public int Radius = 3000;
        public bool IsIncludeWaterPoints = false;

        public LatLng Location
        {
            get
            {
                return new LatLng(Latitude, Longitude);
            }
        }
    }
}
