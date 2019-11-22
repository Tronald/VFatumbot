namespace VFatumbot.BotLogic
{
    public class Consts
    {
#if RELEASE_PROD
        public const string APP_VERSION = "3.1.4";
#else
        public const string APP_VERSION = "3.2.0β";
#endif

        // Azure App ID
#if RELEASE_PROD
        public const string APP_ID = "***REMOVED***";
#else
        public const string APP_ID = "***REMOVED***";
#endif

        // Azure Cosmos DB credentials
        public const string COSMOS_DB_NAME = "***REMOVED***";
        public const string COSMOS_DB_URI = "***REMOVED***";
        public const string COSMOS_DB_KEY = "***REMOVED***";
#if RELEASE_PROD
        public const string COSMOS_CONTAINER_NAME_PERSISTENT = "prod_persistent";
        public const string COSMOS_CONTAINER_NAME_TEMPORARY = "prod_temporary";
#else
        public const string COSMOS_CONTAINER_NAME_PERSISTENT = "dev_persistent";
        public const string COSMOS_CONTAINER_NAME_TEMPORARY = "dev_temporary";
#endif

        // Google Maps API key
        public const string GOOGLE_MAPS_API_KEY = "***REMOVED***";

        // OneSignal for Push Notifications
#if RELEASE_PROD
        public const string ONE_SIGNAL_APP_ID = "***REMOVED***";
        public const string ONE_SIGNAL_API_KEY = "***REMOVED***";
#else
        public const string ONE_SIGNAL_APP_ID = "***REMOVED***";
        public const string ONE_SIGNAL_API_KEY = "***REMOVED***";
#endif

        // Reddit API for posting trip reports
        public const string REDDIT_APP_ID = "***REMOVED***";
        public const string REDDIT_APP_SECRET = "***REMOVED***";
        public const string REDDIT_REFRESH_TOKEN = "***REMOVED***";
        public const string REDDIT_ACCESS_TOKEN = "***REMOVED***";

        // SQL server for posting trip reports/generated points
        public const string DB_SERVER = "***REMOVED***";
        public const string DB_USER = "***REMOVED***";
        public const string DB_PASSWORD = "***REMOVED***";
        public const string DB_NAME = "***REMOVED***";

        // https://what3words.com API key
        public const string W3W_API_KEY = "***REMOVED***";

        // For uploading user trip report photos
        public const string IMGUR_API_CLIENT_ID = "***REMOVED***";
        public const string IMGUR_API_CLIENT_SECRET = "***REMOVED***";

#if RELEASE_PROD
        //public const string PROXY = "***REMOVED***";
        //public const string PROXY = "***REMOVED***";
#else
        //public const string PROXY = "***REMOVED***";
        //public const string PROXY = "***REMOVED***";
#endif

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
        public const string NO_LOCATION_SET_MSG = "You haven't set a location, or it was reset. Send your location from the app (hint: you can do so by tapping the 🌍/::/＋/📎 icon), or type \"search <address/place name>\", or send a Google Maps URL. Don't forget you can type \"help\" for more info.";
    }
}