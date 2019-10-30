using System;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using static VFatumbot.BotLogic.FatumFunctions;
using static VFatumbot.BotLogic.Enums;
using System.IO;
using System.Net;

namespace VFatumbot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        protected readonly ConversationState _conversationState;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, logger)
        {
            _conversationState = conversationState;

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message} {exception.StackTrace}");

                // TODO: if we ever use similar logic to QuantumRandomNumberGenerator.cs to connect to external sources they may also have their exceptions caught here,
                // so consider checking the stacktrace for the class name or ideally come up with a better error handling system than just catching things here coz it's convenient :)
                if ((exception.GetType().Equals(typeof(InvalidDataException)) && "Service did not return random data.".Equals(exception.Message)) ||
                    (exception.GetType().Equals(typeof(WebException)) && exception.Message.Contains("connection attempt failed because the connected party did not properly respond after a period of time"))
                {
                    // qrng.anu seems to have connection issues from our side sometimes?
                    await turnContext.SendActivityAsync("Sorry, there was an error sourcing quantum entropy needed to randomize. Usually when this happens all you need to is try again a little bit later.");
                }
                else
                {
                    // Send a catch-all apology to the user.
                    await turnContext.SendActivityAsync("Sorry, it looks like something went wrong. " + exception.Message + " " + exception.StackTrace);
                }

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }

        public class CallbackOptions
        {
            public bool ResetFlag { get; set; }

            public bool StartTripReportDialog { get; set; }
            public FinalAttractor[] GeneratedPoints { get; set; }
            public string[] Messages { get; set; }
            public PointTypes PointType { get; set; }
            public int[] NumWaterPointsSkipped { get; set; }
            public string[] What3Words { get; set; }

            public bool UpdateIntentSuggestions { get; set; }
            public string[] IntentSuggestions { get; set; }
            public string TimeIntentSuggestionsSet { get; set; }
        }

        // Used as a callback to restart the main dialog (i.e. prompt user for next action) after middleware-intercepted actions
        // (like sending a location or sending "help", which interrupt the normal dialog flow) or long tasks like getting attractors etc.
        // have completed their work on a background thread
        public async Task RepromptMainDialog(ITurnContext turnContext, Dialog dialog, CancellationToken cancellationToken, CallbackOptions callbackOptions = null)
        {
            var conversationStateAccesor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            var dialogSet = new DialogSet(conversationStateAccesor);
            dialogSet.Add(dialog);
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            await dialogContext.ReplaceDialogAsync(nameof(MainDialog), callbackOptions, cancellationToken);
            await _conversationState.SaveChangesAsync(dialogContext.Context, false, cancellationToken);
        }
    }
}