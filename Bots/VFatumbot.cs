using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CoordinateSharp;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome back to Fatumbot!"), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Don't forget to send your current location."), cancellationToken);
                        await _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to Fatumbot. This is a tool to experiment with the ideas that the mind and matter are connected in more ways than currently understood, and that by visiting random (in the true sense of the word) places one can journey outside of their normal probability paths."), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Start off by sending your location, or typing \"search <address>\", or a Google Maps URL. Don't forget you can type \"help\" for more info."), cancellationToken);
                    }

                    // Hack coz Facebook Messenge stopped showing "Send Location" button
                    if (turnContext.Activity.ChannelId.Equals("facebook"))
                    {
                        await turnContext.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
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

            double lat = 0, lon = 0;
            string pushUserId = null;
            if (InterceptPushNotificationSubscription(turnContext, out pushUserId))
            {
                if (userProfile.PushUserId != pushUserId)
                {
                    userProfile.PushUserId = pushUserId;
                    await mUserProfileAccessor.SetAsync(turnContext, userProfile);
                }
            }
            else if (InterceptLocation(turnContext, out lat, out lon)) // Intercept any locations the user sends us, no matter where in the conversation they are
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
                    await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                    //TODO: FatumFunctions.Tolog(message, "locset");

                    await mUserProfileAccessor.SetAsync(turnContext, userProfile);
                    await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, _mainDialog, cancellationToken);

                    return;
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Text.EndsWith("help", StringComparison.InvariantCultureIgnoreCase))
            {
                var reply = MessageFactory.Text(System.IO.File.ReadAllText("help.txt"));
                await turnContext.SendActivityAsync(reply, cancellationToken);
                if (!string.IsNullOrEmpty(turnContext.Activity.Text) && !userProfile.IsLocationSet)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);

                    // Hack coz Facebook Messenge stopped showing "Send Location" button
                    if (turnContext.Activity.ChannelId.Equals("facebook"))
                    {
                        await turnContext.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
                    }

                    return;
                }
                else
                {
                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, _mainDialog, cancellationToken);
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) && !userProfile.IsLocationSet)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);

                // Hack coz Facebook Messenge stopped showing "Send Location" button
                if (turnContext.Activity.ChannelId.Equals("facebook"))
                {
                    await turnContext.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
                }

                return;
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Text.StartsWith("/", StringComparison.InvariantCulture))
            {
                await new ActionHandler().ParseSlashCommands(turnContext, userProfile, cancellationToken, _mainDialog);

                await mUserProfileAccessor.SetAsync(turnContext, userProfile);
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, _mainDialog, cancellationToken);

                return;
            }

            await base.OnTurnAsync(turnContext, cancellationToken);

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

        protected bool InterceptPushNotificationSubscription(ITurnContext turnContext, out string pushUserId)
        {
            pushUserId = null;

            var activity = turnContext.Activity;

            if (activity.Properties != null)
            {
                var pushUserIdFromClient = (string)activity.Properties.GetValue("pushUserId");
                if (!string.IsNullOrEmpty(pushUserIdFromClient))
                {
                    pushUserId = pushUserIdFromClient;
                    return true;
                }
            }

            return false;
        }

        protected bool InterceptLocation(ITurnContext turnContext, out double lat, out double lon)
        {
            lat = lon = Consts.INVALID_COORD;

            var activity = turnContext.Activity;

            bool isFound = false;

            // Prioritize geo coordinates sent via entities
            if (activity.Entities != null)
            {
                foreach (Entity entity in activity.Entities)
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
            if (!isFound && activity.Text != null && (activity.Text.Contains("google.com/maps/") || activity.Text.Contains("Sending location @")))
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
            if (!isFound && !string.IsNullOrEmpty(activity.Text) && activity.Text.ToLower().StartsWith("search", StringComparison.InvariantCulture))
            {
                // dirty hack: get the calling method which is already async to do the Google Geocode async API call
                lat = lon = Consts.INVALID_COORD;
                isFound = true;
            }

            // Fourthly, sometime around late October 2019, about two months after I started coding this bot, Facebook
            // for whatever reason decided to stop displaying the "Location" button that allowed users to easily send
            // their location to us. So here's my workaround that intercepts a shared message sent to the bot from
            // Google Maps with coordinates that we decode here. FYI I could do with some more 酎ハイ right now.
            if (!isFound && activity.ChannelId.Equals("facebook"))
            {
                try
                {
                    /* e.g.
                     * "channelData":{ 
                                      "sender":{ 
                                         "id":"2418280911623293"
                                      },
                                      "recipient":{ 
                                         "id":"422185445016594"
                                      },
                                      "timestamp":1571933853598,
                                      "message":{ 
                                         "mid":"WYvV4Nkos0LXSCT0xhwHz-QWY6PlmXHw1lzdArnJxcDyHORviVvB-22m880-8unGmfNfdwNwANdH4KxHFmVbrQ",
                                         "seq":0,
                                         "is_echo":false,
                                         "attachments":[ 
                                            { 
                                               "type":"fallback",
                                               "title":"33°35'06.1\"N 130°20'24.0\"E",
                                               "url":"https://l.facebook.com/l.php?u=https%3A%2F%2Fgoo.gl%2Fmaps%2Fzzbjm7nutWjmYdDo9&h=AT1d60WwFvNZF-1afhyRyFlCUZLvJqxlw5bgPcYga8z-oi_sA7RO1fn7OwP8Nn29Vi31OTIoWI-aKSTQe-UEJnTzoPA99f5E5nSnb2yZOxYhFNc6EEglmnflNMQ5vBC8KhWEnDt6dw1R&s=1",
                                               "payload":null
                                            }
                                         ]
                                      }
                                   },
                     */
                    JObject channelData = JObject.Parse(activity.ChannelData.ToString());
                    JToken title = channelData["message"]["attachments"][0]["title"];

                    // CoordinateSharp: https://github.com/Tronald/CoordinateSharp
                    Coordinate coordinates = null;
                    if (Coordinate.TryParse(title.ToString(), out coordinates))
                    {
                        lat = coordinates.Latitude.ToDouble();
                        lon = coordinates.Longitude.ToDouble();
                        isFound = true;
                    }
                }
                catch (Exception)
                {
                }
            }

            return isFound;
        }
    }
}
