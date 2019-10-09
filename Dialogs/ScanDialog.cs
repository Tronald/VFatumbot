// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class ScanDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly UserState _userState;

        private MainDialog _mainDialog;

        protected UserProfile UserProfile;

        public ScanDialog(UserState userState, object mainDialog, ILogger<MainDialog> logger) : base(nameof(ScanDialog))
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
            _logger.LogInformation("ScanDialog.ChoiceActionStepAsync");

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            UserProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());

            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose the kind of scan:"),
                RetryPrompt = MessageFactory.Text("That is not a valid scan."),
                Choices = GetActionChoices(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Send a response to the user based on their choice.
        // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ScanDialog.PerformActionStepAsync");

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();
            
            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            var actionHandler = new ActionHandler();

            // Handle the chosen action
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Scan Attractor":
                    await actionHandler.AttractionActionAsync(stepContext, UserProfile, cancellationToken, _mainDialog, true);
                    break;
                case "Scan Void":
                    await actionHandler.VoidActionAsync(stepContext, UserProfile, cancellationToken, _mainDialog, true);
                    break;
                case "Scan Anomaly":
                    await actionHandler.AnomalyActionAsync(stepContext, UserProfile, cancellationToken, _mainDialog, true);
                    break;
                case "Scan Pair":
                    await actionHandler.PairActionAsync(stepContext, UserProfile, cancellationToken, _mainDialog, true);
                    break;
            }

            // Send the reply
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        private IList<Choice> GetActionChoices()
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "Scan Attractor",
                    Synonyms = new List<string>()
                                    {
                                        "scanattractor",
                                        "/scanattractor",
                                    }
                },
                new Choice() {
                    Value = "Scan Void",
                    Synonyms = new List<string>()
                                    {
                                        "scanvoid",
                                        "/scanvoid",
                                    }
                },
                new Choice() {
                    Value = "Scan Anomaly",
                    Synonyms = new List<string>()
                                    {
                                        "scananomaly",
                                        "/scananomaly",
                                    }
                },
                new Choice() {
                    Value = "Scan Pair",
                    Synonyms = new List<string>()
                                    {
                                        "scanpair",
                                        "/scanpair",
                                    }
                },
            };

            return actionOptions;
        }
    }
}
