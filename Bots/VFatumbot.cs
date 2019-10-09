using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public class VFatumbot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly MainDialog Dialog;
        protected readonly ILogger Logger;
        protected readonly BotState UserState;

        protected UserProfile UserProfile;
        
        public VFatumbot(ConversationState conversationState, UserState userState, MainDialog dialog, ILogger<VFatumbot<MainDialog>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
                    UserProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to VFatumbot. Here we're trying at some new features based on the original [Fatum Project bot](https://www.reddit.com/r/randonauts/). Type \"help\" anytime for more info."), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text("Start off by sending your location or a [Google Maps URL](https://www.google.com/maps/@51.509865,-0.118092,13z). Don't forget you can send \"help\" for more info."), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
            UserProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            double lat = 0, lon = 0;
            if (InterceptLocation(turnContext, cancellationToken, out lat, out lon)) // Intercept any locations the user sends us, no matter where in the conversation they are
            {
                // Update user's location
                UserProfile.Latitude = lat;
                UserProfile.Longitude = lon;

                await turnContext.SendActivityAsync(MessageFactory.Text($"New location confirmed @ {lat},{lon}"), cancellationToken);

                var incoords = new double[] { lat, lon };
                var w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);
                await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                //var isWater = await Helpers.IsWaterCoordinatesAsync(new double[] { UserProfile.Latitude, UserProfile.Longitude });
                //await turnContext.SendActivityAsync(MessageFactory.Text($"Is it a water point? {isWater}"), cancellationToken);

                //TODO: FatumFunctions.Tolog(message, "locset");

                await base.OnTurnAsync(turnContext, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) &&
                (
                    turnContext.Activity.Text.EndsWith("help", StringComparison.InvariantCultureIgnoreCase) ||
                    turnContext.Activity.Text.Equals("hi", StringComparison.InvariantCultureIgnoreCase) ||
                    turnContext.Activity.Text.Equals("hello", StringComparison.InvariantCultureIgnoreCase)
                )
            )
            {
                var reply = MessageFactory.Text(System.IO.File.ReadAllText("help.txt"));
                await turnContext.SendActivityAsync(reply, cancellationToken);
                if (!string.IsNullOrEmpty(turnContext.Activity.Text)
                    && UserProfile.Latitude == 0d && UserProfile.Longitude == 0d
                    //&& !"emulator".Equals(turnContext.Activity.ChannelId)
                    )
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("You haven't set a location. Send your location from your messenger app (hint: you can do so by tapping the +/⸬/📎 etc. icon), or if you're on a computer send a [Google Maps URL](https://www.google.com/maps/@51.509865,-0.118092,13z). Don't forget you can send \"help\" for more info."), cancellationToken);
                }
                else
                {
                    await base.OnTurnAsync(turnContext, cancellationToken);
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text)
                && UserProfile.Latitude == 0d && UserProfile.Longitude == 0d
                //&& !"emulator".Equals(turnContext.Activity.ChannelId) 
                )
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("You haven't set a location. Send your location from your messenger app (hint: you can do so by tapping the +/⸬/📎 etc. icon), or if you're on a computer send a [Google Maps URL](https://www.google.com/maps/@51.509865,-0.118092,13z)."), cancellationToken);
            }
            else
            {
                await base.OnTurnAsync(turnContext, cancellationToken);
            }

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            Dialog._conversationState = ConversationState;
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected bool InterceptLocation(ITurnContext turnContext, CancellationToken cancellationToken, out double lat, out double lon)
        {
            lat = lon = 0;
            bool isFound = false;

            // Prioritize geo coordinates sent via entities
            if (turnContext.Activity.Entities != null)
            {
                foreach (Entity entity in turnContext.Activity.Entities)
                {
                    if (entity.Type == "Place")
                    {
                        Place place = entity.GetAs<Place>();
                        GeoCoordinates geo = JsonConvert.DeserializeObject<GeoCoordinates>(place.Geo + "");
                        lat = (double)geo.Latitude;
                        lon = (double)geo.Longitude;
                        isFound = true;
                        break;
                    }
                }
            }

            // Secondly is if there is a Google Map URL
            if (!isFound && turnContext.Activity.Text != null && turnContext.Activity.Text.Contains("google.com/maps/"))
            {
                string[] seps0 = { "@" };
                string[] entry0 = turnContext.Activity.Text.Split(seps0, StringSplitOptions.RemoveEmptyEntries);
                string[] seps = { "," };
                string[] entry = entry0[1].Split(seps, StringSplitOptions.RemoveEmptyEntries);
                if (Double.TryParse(entry[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat) &&
                    Double.TryParse(entry[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon))
                {
                    isFound = true;
                }
            }

            // TODO: take any unexpected input to be an address and search its lats/longs on the Google Map API? Have to take into consideration the dialog state.

            return isFound;
        }
    }
}
