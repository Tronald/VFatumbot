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
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly UserState _userState;

        public UserProfile UserProfile;

        public MainDialog(UserState userState, ConversationState conversationState, ILogger<MainDialog> logger) : base(nameof(MainDialog))
        {
            _logger = logger;
            _userState = userState;

            // Define the main dialog and its related components.
            AddDialog(new BlindSpotsDialog(userState, this, logger));
            AddDialog(new ReportDialog(userState, this, logger));
            AddDialog(new ScanDialog(userState, this, logger));
            AddDialog(new SettingsDialog(userState, this, logger));
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
        private async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ChoiceActionStepAsync");

            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What would you like to get/check?"),
                RetryPrompt = MessageFactory.Text("That is not a valid action. What would you like to get/check?"),
                Choices = GetActionChoices(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        // Send a response to the user based on their choice.
        // This method is only called when a valid prompt response is parsed from the user's response to the ChoicePrompt.
        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.PerformActionStepAsync");

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();
            
            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            var actionHandler = new ActionHandler();

            var repromptThisRound = false;

            // Handle the chosen action
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Attractor":
                    await actionHandler.AttractionActionAsync(stepContext, UserProfile, cancellationToken, this);
                    break;
                case "Void":
                    await actionHandler.VoidActionAsync(stepContext, UserProfile, cancellationToken, this);
                    break;
                case "Anomaly":
                    await actionHandler.AnomalyActionAsync(stepContext, UserProfile, cancellationToken, this);
                    break;
                case "Intent Suggestions":
                    await actionHandler.IntentSuggestionActionAsync(stepContext, UserProfile, cancellationToken, this);
                    break;
                case "Pair":
                    await actionHandler.PairActionAsync(stepContext, UserProfile, cancellationToken, this);
                    break;
                case "Blind Spots":
                    return await stepContext.BeginDialogAsync(nameof(BlindSpotsDialog), this, cancellationToken);
                case "Scan":
                    return await stepContext.BeginDialogAsync(nameof(ScanDialog), this, cancellationToken);
                case "My Location":
                    // TODO: we shouldn't need this location-is-set check here because it's checked higher up in VFatumbot.cs but the location wasn't been set sometimes so just for debugging now...
                    if (!UserProfile.IsLocationSet)
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);
                        await stepContext.RepromptDialogAsync(cancellationToken);
                    }
                    // END TODO

                    repromptThisRound = true;
                    await actionHandler.LocationActionAsync(stepContext, UserProfile, cancellationToken);
                    break;
                case "Settings":
                    return await stepContext.BeginDialogAsync(nameof(SettingsDialog), this, cancellationToken);
            }

            // Send the reply
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            if (repromptThisRound)
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
                    Value = "Attractor",
                    Synonyms = new List<string>()
                                    {
                                        "attractor",
                                        "getattractor",
                                        "/getattractor",
                                    }
                },
                new Choice() {
                    Value = "Void",
                    Synonyms = new List<string>()
                                    {
                                        "void",
                                        "getvoid",
                                        "/getvoid",
                                        "Repeller",
                                        "repeller",
                                        "getrepeller",
                                        "/getrepeller"
                                    }
                },
                new Choice() {
                    Value = "Anomaly",
                    Synonyms = new List<string>()
                                    {
                                        "anomaly",
                                        "getanomaly",
                                        "/getanomaly",
                                        "ida",
                                        "getida",
                                        "/getida",
                                    }
                },
                new Choice() {
                    Value = "Intent Suggestions",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
                new Choice() {
                    Value = "Pair",
                    Synonyms = new List<string>()
                                    {
                                        "pair",
                                        "getpair",
                                        "/getpair",
                                    }
                },
                new Choice() {
                    Value = "Blind Spots",
                    Synonyms = new List<string>()
                                    {
                                        "blind spots",
                                        "blindspots",
                                        "/blindspots",
                                    }
                },
                new Choice() {
                    Value = "Scan",
                    Synonyms = new List<string>()
                                    {
                                        "scan",
                                        "/scan",
                                    }
                },
                new Choice() {
                    Value = "My Location",
                    Synonyms = new List<string>()
                                    {
                                        "My Location",
                                        "My location",
                                        "my location",
                                        "location",
                                        "setlocation",
                                        "/setlocation",
                                    }
                },
                 new Choice() {
                    Value = "Settings",
                    Synonyms = new List<string>()
                                    {
                                        "settings",
                                    }
                },
            };

            return actionOptions;
        }
    }
}
