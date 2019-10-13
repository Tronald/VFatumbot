using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public class BlindSpotsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly UserState _userState;

        private MainDialog _mainDialog;

        protected UserProfile UserProfile;

        public BlindSpotsDialog(UserState userState, object mainDialog, ILogger<MainDialog> logger) : base(nameof(BlindSpotsDialog))
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
            _logger.LogInformation("BlindSpotsDialog.ChoiceActionStepAsync");

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            UserProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose the kind of blind spot:"),
                RetryPrompt = MessageFactory.Text("That is not a valid blind spot."),
                Choices = GetActionChoices(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Send a response to the user based on their choice.
        // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("BlindSpotsDialog.PerformActionStepAsync");

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();
            
            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            var actionHandler = new ActionHandler();

            var goBackMainMenuThisRound = false;

            // Handle the chosen action
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Quantum":
                    goBackMainMenuThisRound = true;
                    await actionHandler.QuantumActionAsync(stepContext, UserProfile, cancellationToken);
                    break;
                case "Quantum Time":
                    goBackMainMenuThisRound = true;
                    await actionHandler.QuantumActionAsync(stepContext, UserProfile, cancellationToken, true);
                    break;
                case "Psuedo":
                    goBackMainMenuThisRound = true;
                    await actionHandler.PsuedoActionAsync(stepContext, UserProfile, cancellationToken);
                    break;
                case "Point":
                    await actionHandler.PointActionAsync(stepContext, UserProfile, cancellationToken, _mainDialog);
                    break;
                case "< Back":
                    goBackMainMenuThisRound = true;
                    break;
            }

            // Send the reply
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            if (goBackMainMenuThisRound)
            {
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
            }
            else
            {
                // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
                return await stepContext.EndDialogAsync();
            }
        }

        private IList<Choice> GetActionChoices()
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "Quantum",
                    Synonyms = new List<string>()
                                    {
                                        "quantum",
                                        "getquantum",
                                        "/getquantum",
                                    }
                },
                new Choice() {
                    Value = "Quantum Time",
                    Synonyms = new List<string>()
                                    {
                                        "quantumtime",
                                        "getquantumtime",
                                        "qtime",
                                        "/getqtime",
                                    }
                },
                new Choice() {
                    Value = "Psuedo",
                    Synonyms = new List<string>()
                                    {
                                        "psuedo",
                                        "getpsuedo",
                                        "/getpsuedo",
                                    }
                },
                new Choice() {
                    Value = "Point",
                    Synonyms = new List<string>()
                                    {
                                        "point",
                                        "getpoint",
                                        "/getpoint",
                                    }
                },
                new Choice() {
                    Value = "< Back",
                    Synonyms = new List<string>()
                                    {
                                        "<",
                                        "Back",
                                        "back",
                                        "<back",
                                    }
                },
            };

            return actionOptions;
        }
    }
}
