namespace VFatumbot.BotLogic
{
    public class Consts
    {
        // Azure App ID
        public const string APP_ID = "";

        // Azure Cosmos DB credentials
        public const string COSMOS_DB_NAME = "";
        public const string COSMOS_CONTAINER_NAME = "";
        public const string COSMOS_DB_URI = "";
        public const string COSMOS_DB_KEY = "";

        // Google Maps API key
        public const string GOOGLE_MAPS_API_KEY = "";

        // https://what3words.com API key
        public const string W3W_API_KEY = "";

        // Google Maps etc thumbnail sizes to use in reply cards
        public const string THUMBNAIL_SIZE = "400x400";

        // Dummy invalid coordinate
        public const double INVALID_COORD = -1000.0;

        // Default radius to search within (meters)
        public const int DEFAULT_RADIUS = 3000;

        // TODO: move later when localization is implemented
        public const string NO_LOCATION_SET_MSG = "You haven't set a location. Send your location from (hint: you can do so by tapping the 🌍/::/＋/📎 icon in your app), or type \"search <address>\", or send a [Google Maps URL](https://www.google.com/maps/@51.509865,-0.118092,13z). Don't forget you can type \"help\" for more info.";
    }
}
