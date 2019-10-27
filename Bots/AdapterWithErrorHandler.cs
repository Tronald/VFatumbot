using System;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        protected readonly ConversationState  _conversationState;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, logger)
        {
            _conversationState = conversationState;

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message} {exception.StackTrace}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong. " + exception.Message + " " + exception.StackTrace);

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
            public int NumWaterPointsSkipped { get; set; }
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