using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    // Defines a state property used to track information about the user that is persisted
    public class UserProfilePersistent
    {
        public string UserId { get; set; }

        public bool IsIncludeWaterPoints { get; set; } = true;
        public bool IsDisplayGoogleThumbnails { get; set; } = false;

        // OneSignal Player/User ID for push notifications
        public string PushUserId { get; set; }

        // Kind of track whether they've used the bot before
        public bool HasSetLocationOnce { get; set;} = false;
    }
}
