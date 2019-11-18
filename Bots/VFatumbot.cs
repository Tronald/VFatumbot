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
using static VFatumbot.BotLogic.Enums;

namespace VFatumbot
{
    public class VFatumbot<T> : ActivityHandler where T : Dialog
    {
        protected readonly MainDialog _mainDialog;
        protected readonly ILogger _logger;
        protected readonly ConversationState _conversationState;
        protected readonly UserPersistentState _userPersistentState;
        protected readonly UserTemporaryState _userTemporaryState;

        protected IStatePropertyAccessor<ConversationData> _conversationDataAccessor;
        protected IStatePropertyAccessor<UserProfilePersistent> _userProfilePersistentAccessor;
        protected IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;

        public VFatumbot(ConversationState conversationState, UserPersistentState userPersistentState, UserTemporaryState userTemporaryState, MainDialog dialog, ILogger<VFatumbot<MainDialog>> logger)
        {
            _conversationState = conversationState;
            _userPersistentState = userPersistentState;
            _userTemporaryState = userTemporaryState;
            _mainDialog = dialog;
            _logger = logger;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var userProfilePersistent = await _userProfilePersistentAccessor.GetAsync(turnContext, () => new UserProfilePersistent());
                    var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(turnContext, () => new UserProfileTemporary());

                    if (userProfileTemporary.IsLocationSet)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome back to Fatumbot!"), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Don't forget to send your current location."), cancellationToken);
                        await _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                    }
                    else if (userProfilePersistent.HasSetLocationOnce)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome back to Fatumbot!"), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to Fatumbot. This is a tool to experiment with the ideas that the mind and matter are connected in more ways than currently understood, and that by visiting random (in the true sense of the word) places one can journey outside of their normal probability paths."), cancellationToken);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Start off by sending your location from the app (hint: you can do so by tapping the 🌍/::/＋/📎 icon in your app), or type \"search <address>\", or send a Google Maps URL. Don't forget you can type \"help\" for more info."), cancellationToken);
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
            _userProfilePersistentAccessor = _userPersistentState.CreateProperty<UserProfilePersistent>(nameof(UserProfilePersistent));
            var userProfilePersistent = await _userProfilePersistentAccessor.GetAsync(turnContext, () => new UserProfilePersistent());

            _userProfileTemporaryAccessor = _userTemporaryState.CreateProperty<UserProfileTemporary>(nameof(UserProfileTemporary));
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(turnContext, () => new UserProfileTemporary());

            _conversationDataAccessor = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await _conversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

            // Print info about image attachments
            //if (turnContext.Activity.Attachments != null)
            //{
            //    await turnContext.SendActivityAsync(JsonConvert.SerializeObject(turnContext.Activity.Attachments), cancellationToken: cancellationToken);
            //}

            // Save user's ID
            userProfilePersistent.UserId = userProfileTemporary.UserId = Helpers.Sha256Hash(turnContext.Activity.From.Id);

            // Add message details to the conversation data.
            var messageTimeOffset = (DateTimeOffset)turnContext.Activity.Timestamp;
            var localMessageTime = messageTimeOffset.ToLocalTime();
            conversationData.Timestamp = localMessageTime.ToString();
            await _conversationDataAccessor.SetAsync(turnContext, conversationData);

            // TODO: most of the logic/functionalioty in the following if statements I realised later on should probably be structured in the way the Bot Framework SDK talks about "middleware".
            // Maybe one day re-structure/re-factor it to following their middleware patterns...

            double lat = 0, lon = 0;
            string pushUserId = null;
            userProfileTemporary.PushUserId = userProfilePersistent.PushUserId;
            if (InterceptPushNotificationSubscription(turnContext, out pushUserId))
            {
                if (userProfilePersistent.PushUserId != pushUserId)
                {
                    userProfilePersistent.PushUserId = userProfileTemporary.PushUserId = pushUserId;
                    await _userProfilePersistentAccessor.SetAsync(turnContext, userProfilePersistent);
                    await _userProfileTemporaryAccessor.SetAsync(turnContext, userProfileTemporary);
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
                    userProfileTemporary.Latitude = lat;
                    userProfileTemporary.Longitude = lon;

                    await turnContext.SendActivityAsync(MessageFactory.Text($"New location confirmed @ {lat},{lon}"), cancellationToken);

                    var incoords = new double[] { lat, lon };
                    var w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);
                    await turnContext.SendActivitiesAsync(CardFactory.CreateLocationCardsReply(Enum.Parse<ChannelPlatform>(turnContext.Activity.ChannelId), incoords, userProfileTemporary.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);

                    await _userProfileTemporaryAccessor.SetAsync(turnContext, userProfileTemporary);
                    await _userTemporaryState.SaveChangesAsync(turnContext, false, cancellationToken);

                    userProfilePersistent.HasSetLocationOnce = true;
                    await _userProfilePersistentAccessor.SetAsync(turnContext, userProfilePersistent);
                    await _userPersistentState.SaveChangesAsync(turnContext, false, cancellationToken);

                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, _mainDialog, cancellationToken);

                    return;
                }
            }
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Text.EndsWith("help", StringComparison.InvariantCultureIgnoreCase))
            {
#if RELEASE_PROD
                var help = System.IO.File.ReadAllText("help-prod.txt").Replace("APP_VERSION", Consts.APP_VERSION);
#else
                var help = System.IO.File.ReadAllText("help-dev.txt").Replace("APP_VERSION", Consts.APP_VERSION);
#endif
                if (turnContext.Activity.ChannelId.Equals("telegram"))
                {
                    /* Beta test report from Telegram users sending the help command:
                     * Sorry, it looks like something went wrong. ErrorResponseException: Operation returned an invalid status code 'BadRequest'    at Microsoft.Bot.Connector.Conversations.ReplyToActivityWithHttpMessagesAsync(String conversationId, String activityId, Activity activity, Dictionary2 customHeaders, CancellationToken cancellationToken)    at Microsoft.Bot.Connector.ConversationsExtensions.ReplyToActivityAsync(IConversations operations, String conversationId, String activityId, Activity activity, CancellationToken cancellationToken)    at Microsoft.Bot.Builder.BotFrameworkAdapter.SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)    at Microsoft.Bot.Builder.TurnContext.<>c__DisplayClass22_0.<<SendActivitiesAsync>g__SendActivitiesThroughAdapter|1>d.MoveNext() --- End of stack trace from previous location where exception was thrown ---    at Microsoft.Bot.Builder.TurnContext.SendActivityAsync(IActivity activity, CancellationToken cancellationToken)    at VFatumbot.VFatumbot1.OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken) in /Users/simon/Dropbox (Personal)/Fatum/github/VFatumbot/Bots/VFatumbot.cs:line 142    at Microsoft.Bot.Builder.BotFrameworkAdapter.TenantIdWorkaroundForTeamsMiddleware.OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)    at Microsoft.Bot.Builder.MiddlewareSet.ReceiveActivityWithStatusAsync(ITurnContext turnContext, BotCallbackHandler callback, CancellationToken cancellationToken)    at Microsoft.Bot.Builder.BotAdapter.RunPipelineAsync(ITurnContext turnContext, BotCallbackHandler callback, CancellationToken cancellationToken)
                     * I think the message is too long so will split it up here...
                     * another drunk quick fix :D
                     */

                    var first = help.Substring(0, (int)(help.Length / 2));
                    var last = help.Substring((int)(help.Length / 2), (int)(help.Length / 2));

                    await turnContext.SendActivityAsync(MessageFactory.Text(first), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text(last), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(help), cancellationToken);
                }
                if (!string.IsNullOrEmpty(turnContext.Activity.Text) && !userProfileTemporary.IsLocationSet)
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
            else if (!string.IsNullOrEmpty(turnContext.Activity.Text) && !userProfileTemporary.IsLocationSet)
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
                await new ActionHandler().ParseSlashCommands(turnContext, userProfileTemporary, cancellationToken, _mainDialog);

                await _userProfileTemporaryAccessor.SetAsync(turnContext, userProfileTemporary);
                await _userPersistentState.SaveChangesAsync(turnContext, false, cancellationToken);
                await _userTemporaryState.SaveChangesAsync(turnContext, false, cancellationToken);

                return;
            }

            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userPersistentState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userTemporaryState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Running dialog with Message Activity.");

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
