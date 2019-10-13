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
        protected BotState ConversationState;
        protected MainDialog Dialog;
        protected ILogger Logger;
        protected BotState UserState;

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

                    if (UserProfile.IsLocationSet)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome back to VFatumbot!"), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Don't forget to send your current location."), cancellationToken);
                        Dialog.UserProfile = UserProfile;
                        await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to VFatumbot. Here we're trying at some new features based on the original [Fatum Project bot](https://www.reddit.com/r/randonauts/). Type \"help\" anytime for more info."), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Start off by sending your location, or typing \"search <address>\", or a [Google Maps URL](https://www.google.com/maps/@51.509865,-0.118092,13z). Don't forget you can type \"help\" for more info."), cancellationToken);
                    }
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
            UserProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            var conversationStateAccessors = ConversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            double lat = 0, lon = 0;
            if (InterceptLocation(turnContext, out lat, out lon)) // Intercept any locations the user sends us, no matter where in the conversation they are
            {
                bool cont = true;
                if (lat == Consts.INVALID_COORD && lon == Consts.INVALID_COORD)
                {
                    // Do a geocode query lookup against the address the user sent
                    var result = await Helpers.GeocodeAddressAsync(turnContext.Activity.Text.ToLower().Replace("search", ""));
                    if (result != null)
                    {
                        lat = result.Item1;
                        lon = result.Item2;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Address not found."), cancellationToken);
                        cont = false;
                    }
                }

                if (cont)
                {
                    // Update user's location
                    UserProfile.Latitude = lat;
                    UserProfile.Longitude = lon;

                    await turnContext.SendActivityAsync(MessageFactory.Text($"New location confirmed @ {lat},{lon}"), cancellationToken);

                    var incoords = new double[] { lat, lon };
                    var w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);
                    await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                    //TODO: FatumFunctions.Tolog(message, "locset");
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) &&
                (
                    turnContext.Activity.Text.EndsWith("help", StringComparison.InvariantCultureIgnoreCase)
                )
            )
            {
                var reply = MessageFactory.Text(System.IO.File.ReadAllText("help.txt"));
                await turnContext.SendActivityAsync(reply, cancellationToken);
                if (!string.IsNullOrEmpty(turnContext.Activity.Text)
                    && !UserProfile.IsLocationSet
                    )
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);
                }
                else
                {
                    await base.OnTurnAsync(turnContext, cancellationToken);
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text)
                && !UserProfile.IsLocationSet
                )
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);
            }
            else
            {
                await base.OnTurnAsync(turnContext, cancellationToken);
            }

            // Save user's IDs
            UserProfile.UserId = turnContext.Activity.From.Id;
            UserProfile.Username = turnContext.Activity.From.Name;

            // Add message details to the conversation data.
            // Convert saved Timestamp to local DateTimeOffset, then to string for display.
            var messageTimeOffset = (DateTimeOffset)turnContext.Activity.Timestamp;
            var localMessageTime = messageTimeOffset.ToLocalTime();
            conversationData.Timestamp = localMessageTime.ToString();
            conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            Dialog.UserProfile = UserProfile;
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected bool InterceptLocation(ITurnContext turnContext, out double lat, out double lon)
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
            if (!isFound && turnContext.Activity.Text != null && (turnContext.Activity.Text.Contains("google.com/maps/") || turnContext.Activity.Text.Contains("Sending location @")))
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

            // Geocode the address the user sent
            if (!isFound && !string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Text.ToLower().StartsWith("search"))
            {
                // dirty hack: get the calling method which is already async to do the Google Geocode async API call
                lat = lon = Consts.INVALID_COORD;

                return true;
            }

            return isFound;
        }
    }
}
