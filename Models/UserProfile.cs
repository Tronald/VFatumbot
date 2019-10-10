using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {

        public double Latitude = Consts.INVALID_COORD;
        public double Longitude = Consts.INVALID_COORD;
        public int Radius = Consts.DEFAULT_RADIUS;
        public bool IsIncludeWaterPoints = false;

        public bool IsLocationSet
        {
            get
            {
                return Latitude != Consts.INVALID_COORD && Longitude != Consts.INVALID_COORD;
            }
        }

        public LatLng Location
        {
            get
            {
                return new LatLng(Latitude, Longitude);
            }
        }
    }
}
