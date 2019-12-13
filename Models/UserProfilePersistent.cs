using System;
using Microsoft.Bot.Builder;
using VFatumbot.BotLogic;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    public class UserPersistentState : BotState {
        public UserPersistentState(IStorage storage) : base(storage, nameof(UserPersistentState)) { }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/users/{userId}";
        }
    }
    
    // Defines a state property used to track information about the user that is persisted
    public class UserProfilePersistent
    {
        public string UserId { get; set; }

        public bool IsIncludeWaterPoints { get; set; } = true;
        public bool IsDisplayGoogleThumbnails { get; set; } = false;

        // OneSignal Player/User ID for push notifications
        public string PushUserId { get; set; }

        // Kind of track whether they've used the bot before
        public bool HasSetLocationOnce { get; set; } = false;

        public bool HasAgreedToToS { get; set; } = false;
    }
}
