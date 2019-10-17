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
        protected readonly ConversationState _conversationState;
        protected readonly MainDialog _mainDialog;
        protected readonly ILogger _logger;
        protected readonly UserState _userState;

        protected IStatePropertyAccessor<UserProfile> mUserProfileAccessor;
        protected IStatePropertyAccessor<ConversationData> mConversationDataAccessor;

        public VFatumbot(ConversationState conversationState, UserState userState, MainDialog dialog, ILogger<VFatumbot<MainDialog>> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _mainDialog = dialog;
            _logger = logger;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var userProfile = await mUserProfileAccessor.GetAsync(turnContext, () => new UserProfile());

                    if (userProfile.IsLocationSet)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome back to VFatumbot!"), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Don't forget to send your current location."), cancellationToken);
                        await _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
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
            mUserProfileAccessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await mUserProfileAccessor.GetAsync(turnContext, () => new UserProfile());

            mConversationDataAccessor = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await mConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

            // Save user's IDs
            userProfile.UserId = turnContext.Activity.From.Id;
            userProfile.Username = turnContext.Activity.From.Name;

            // Add message details to the conversation data.
            var messageTimeOffset = (DateTimeOffset)turnContext.Activity.Timestamp;
            var localMessageTime = messageTimeOffset.ToLocalTime();
            conversationData.Timestamp = localMessageTime.ToString();
            conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();
            await mConversationDataAccessor.SetAsync(turnContext, conversationData);

            // TODO: most of the logic/functionalioty in the following if statements I realised later on should probably be structured in the way the Bot Framework SDK talks about "middleware".
            // Maybe one day re-structure/re-factor it to following their middleware patterns...

            bool abortTurn = false;

            double lat = 0, lon = 0;
            if (InterceptLocation(turnContext, out lat, out lon)) // Intercept any locations the user sends us, no matter where in the conversation they are
            {
                bool validCoords = true;
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
                        validCoords = false;
                    }
                }

                if (validCoords)
                {
                    // Update user's location
                    userProfile.Latitude = lat;
                    userProfile.Longitude = lon;

                    await turnContext.SendActivityAsync(MessageFactory.Text($"New location confirmed @ {lat},{lon}"), cancellationToken);

                    var incoords = new double[] { lat, lon };
                    var w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);
                    await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                    //TODO: FatumFunctions.Tolog(message, "locset");
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) &&
                    turnContext.Activity.Text.EndsWith("help", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                var reply = MessageFactory.Text(System.IO.File.ReadAllText("help.txt"));
                await turnContext.SendActivityAsync(reply, cancellationToken);
                if (!string.IsNullOrEmpty(turnContext.Activity.Text)
                    && !userProfile.IsLocationSet
                    )
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);
                    abortTurn = true;
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text)
                    && !userProfile.IsLocationSet
            )
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);
                abortTurn = true;
            }

            if (!abortTurn)
            {
                await mUserProfileAccessor.SetAsync(turnContext, userProfile);
                await base.OnTurnAsync(turnContext, cancellationToken);
            }

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the MainDialog with the new message Activity
            await _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
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

            // Thirdly, geocode the address the user sent
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
