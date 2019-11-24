using System;
using Microsoft.Bot.Builder;
using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    public class UserTemporaryState : BotState
    {
        public UserTemporaryState(IStorage storage) : base(storage, nameof(UserTemporaryState)) { }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/users/{userId}";
        }
    }

    // Defines a state property used to track information about the user that is only stored temporarily (stuff that should be reset every now and then).
    public class UserProfileTemporary
    {
        // TODO: I split UserProfile into Persistent and Temporary so Temporary could help us strengthen our approach to privacy and anonymity by
        // leveraging the Time-To-Live (TTL) feature of the Azure Cosmos DB (NOSQL) database. Basically the temporary state property holds stuff
        // like users' current location which is private data we have no need do hold onto permanently. But by the time I got to this stage of dev
        // and with all the shit going on I decided to write probably the longest comment in this whole codebase.
        // TL/DR; to avoid having to refactor a shit load we just temporarily set some of the persistent stuff like PushId and some settings here.
        public string UserId { get; set; }
        public string PushUserId { get; set; }
        public bool IsIncludeWaterPoints { get; set; } = true;
        public bool IsDisplayGoogleThumbnails { get; set; } = false;

#if EMULATORDEBUG
        // Fukuoka, Japan
        public double Latitude { get; set; } = 33.5977505;
        public double Longitude { get; set; } = 130.409509;
#else
        public double Latitude = Consts.INVALID_COORD;
        public double Longitude = Consts.INVALID_COORD;
#endif
        public int Radius { get; set; } = Consts.DEFAULT_RADIUS;

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

        public void ResetLocation()
        {
            Latitude = Longitude = Consts.INVALID_COORD;
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

        public string[] IntentSuggestions { get; set; }
        public string TimeIntentSuggestionsSet { get; set; }
    }
}
