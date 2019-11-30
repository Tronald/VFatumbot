using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
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

                    if (Helpers.IsRandoLobby(turnContext))
                    {
                        // If Randonauts Telegram lobby then keep the welcome short
                        Activity replyActivity;
                        if (userProfileTemporary.IsLocationSet || userProfilePersistent.HasSetLocationOnce)
                        {
                            replyActivity = MessageFactory.Text($"Welcome back @{turnContext.Activity.From.Name}!");
                        }
                        else
                        {
                            replyActivity = MessageFactory.Text($"Welcome @{turnContext.Activity.From.Name}! Message the @shangrila_bot privately to start your adventure and feel free to share your experiences here.");
                        }

                        await turnContext.SendActivityAsync(replyActivity);
                    }
                    else if (userProfileTemporary.IsLocationSet)
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
                        await turnContext.SendActivityAsync(MessageFactory.Text("Start off by sending your location from the app (hint: you can do so by tapping the 🌍/::/＋/📎 icon), or type \"search <address/place name>\", or send a Google Maps URL. Don't forget you can type \"help\" for more info."), cancellationToken);
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
            else if (Helpers.InterceptLocation(turnContext, out lat, out lon)) // Intercept any locations the user sends us, no matter where in the conversation they are
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
                        await turnContext.SendActivityAsync(MessageFactory.Text("Place not found."), cancellationToken);
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
                await Helpers.HelpAsync(turnContext, userProfileTemporary, _mainDialog, cancellationToken);
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
            await _mainDialog.Run(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
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
    }
}
