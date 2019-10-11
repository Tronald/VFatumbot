using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
#if EMUALATOR
        public double Latitude = Consts.INVALID_COORD;
        public double Longitude = Consts.INVALID_COORD;
#else
        // Fukuoka, Japan
        public double Latitude = 33.5977505;
        public double Longitude = 130.409509;
#endif
        public int Radius = Consts.DEFAULT_RADIUS;
        public bool IsIncludeWaterPoints = false;

        public bool IsLocationSet
        {
            get
            {
#if EMUALATOR
                return true;
#else
                return Latitude != Consts.INVALID_COORD && Longitude != Consts.INVALID_COORD;
#endif
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
