using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace VFatumbot
{
    public class ReportDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly UserState _userState;

        private MainDialog _mainDialog;

        protected UserProfile UserProfile;

        public ReportDialog(UserState userState, object mainDialog, ILogger<MainDialog> logger) : base(nameof(ReportDialog))
        {
            _logger = logger;
            _userState = userState;
            _mainDialog = (MainDialog)mainDialog;

            // Define the main dialog and its related components.
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceActionStepAsync,
                PerformActionStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // 1. Prompts the user if the user is not in the middle of a dialog.
        // 2. Re-prompts the user when an invalid input is received.
        public async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ReportDialog.ChoiceActionStepAsync");

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            UserProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("(IN-DEV, not functioning yet) Did you visit this point and would you like to report on your experience?"),
                RetryPrompt = MessageFactory.Text("That is not a valid answer."),
                Choices = GetActionChoices(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Send a response to the user based on their choice.
        // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ReportDialog.PerformActionStepAsync");

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();
            
            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Handle the chosen action
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "No":
                    return await stepContext.EndDialogAsync();
                    break;
                case "Yes and report":
                    return await stepContext.EndDialogAsync();
                    break;
                case "Yes sans report":
                    return await stepContext.EndDialogAsync();
                    break;
            }

            // Send the reply
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private IList<Choice> GetActionChoices()
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "No",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
                new Choice() {
                    Value = "Yes and report",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
                new Choice() {
                    Value = "Yes sans report",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
            };

            return actionOptions;
        }
    }
}
