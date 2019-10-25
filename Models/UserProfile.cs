using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public string UserId { get; set; }
        public string Username { get; set; }

#if EMULATORDEBUG
        // Fukuoka, Japan
        public double Latitude { get; set; } = 33.5977505;
        public double Longitude { get; set; } = 130.409509;
#else
        public double Latitude = Consts.INVALID_COORD;
        public double Longitude = Consts.INVALID_COORD;
#endif
        public int Radius { get; set; } = Consts.DEFAULT_RADIUS;
        public bool IsIncludeWaterPoints { get; set; } = true;

        // OneSignal Player/User ID for push notifications
        public string PushUserId { get; set; }

        public bool IsLocationSet
        {
            get
            {
#if EMULATORDEBUG
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

        // Flag to prevent multiple parallel scans (based on original Fatumbot3 logic)
        public bool IsScanning { get; set; }
    }
}
