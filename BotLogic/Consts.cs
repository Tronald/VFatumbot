namespace VFatumbot.BotLogic
{
    public class Consts
    {
        // Azure App ID

//#if RELEASE_PROD
        public const string APP_ID = "";
/*#else
        public const string APP_ID = "";
#endif*/

        // Azure Cosmos DB credentials
//#if RELEASE_PROD
        public const string COSMOS_DB_NAME = "";
        public const string COSMOS_CONTAINER_NAME = "";
        public const string COSMOS_DB_URI = "";
        public const string COSMOS_DB_KEY = "";
/*#else
        public const string COSMOS_DB_NAME = "";
        public const string COSMOS_CONTAINER_NAME = "";
        public const string COSMOS_DB_URI = "";
        public const string COSMOS_DB_KEY = "";
#endif*/

        // Google Maps API key
        public const string GOOGLE_MAPS_API_KEY = "";

        // OneSignal for Push Notifications
        public const string ONE_SIGNAL_APP_ID = "";
        public const string ONE_SIGNAL_API_KEY = "";

        // Reddit API for posting trip reports
        public const string REDDIT_APP_ID = "";
        public const string REDDIT_APP_SECRET = "";
        public const string REDDIT_REFRESH_TOKEN = "";
        public const string REDDIT_ACCESS_TOKEN = "";

        // https://what3words.com API key
        public const string W3W_API_KEY = "";

        // Google Maps etc thumbnail sizes to use in reply cards
        public const string THUMBNAIL_SIZE = "400x400";

        // Dummy invalid coordinate
        public const double INVALID_COORD = -1000.0;

        // Default radius to search within (meters)
        public const int DEFAULT_RADIUS = 3000;

        // Max radius
        public const int RADIUS_MAX = 100000;

        // Min radius
        public const int RADIUS_MIN = 1000;

        // Maximum number of tries to search for non-water points before giving up
        public const int WATER_POINTS_SEARCH_MAX = 10;

        // TODO: move later when localization is implemented
        public const string NO_LOCATION_SET_MSG = "You haven't set a location, or it was reset. Send your location from the app (hint: you can do so by tapping the 🌍/::/＋/📎 icon in your app), or type \"search <address>\", or send a Google Maps URL. Don't forget you can type \"help\" for more info.";
    }
}