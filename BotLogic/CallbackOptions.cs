using static VFatumbot.BotLogic.Enums;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot.BotLogic
{
    public class CallbackOptions
    {
        public bool ResetFlag { get; set; }

        public bool StartTripReportDialog { get; set; }
        public string[] ShortCodes { get; set; } // short hash IDs (using CRC32 so the string isn't too long)
        public FinalAttractor[] GeneratedPoints { get; set; }
        public string[] Messages { get; set; }
        public PointTypes[] PointTypes { get; set; }
        public int[] NumWaterPointsSkipped { get; set; }
        public string[] What3Words { get; set; }
        public string[] NearestPlaces { get; set; }

        public bool UpdateIntentSuggestions { get; set; }
        public string[] IntentSuggestions { get; set; }
        public string TimeIntentSuggestionsSet { get; set; }

        public bool UpdateSettings { get; set; }
    }
}
