using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using VFatumbot.BotLogic;
using static VFatumbot.AdapterWithErrorHandler;

namespace VFatumbot
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly UserState _userState;
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public MainDialog(UserState userState, ConversationState conversationState, ILogger<MainDialog> logger) : base(nameof(MainDialog))
        {
            _logger = logger;
            _userState = userState;

            if (userState != null)
                _userProfileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));

            AddDialog(new BlindSpotsDialog(_userProfileAccessor, this, logger));
            AddDialog(new TripReportDialog(_userProfileAccessor, logger));
            AddDialog(new ScanDialog(_userProfileAccessor, this, logger));
            AddDialog(new SettingsDialog(_userProfileAccessor, logger));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceActionStepAsync,
                PerformActionStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Options != null)
            {
                // Callback options passed after resuming dialog after long-running background threads etc have finished
                // and resume dialog via the Adapter class's callback method.
                var callbackOptions = (CallbackOptions)stepContext.Options;

                if (callbackOptions.ResetFlag)
                {
                    var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
                    userProfile.IsScanning = false;
                    await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
                    await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }

                if (callbackOptions.StartTripReportDialog)
                {
                    return await stepContext.ReplaceDialogAsync(nameof(TripReportDialog), callbackOptions, cancellationToken);
                }
            }

            _logger.LogInformation("MainDialog.ChoiceActionStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What would you like to get/check?"),
                RetryPrompt = MessageFactory.Text("That is not a valid action. What would you like to get/check?"),
                Choices = GetActionChoices(stepContext.Context),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"MainDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            var actionHandler = new ActionHandler();
            var repromptThisRound = false;

            switch (((FoundChoice)stepContext.Result).Value)
            {
                // Hack coz Facebook Messenge stopped showing "Send Location" button
                case "Set Location":
                    repromptThisRound = true;
                    await stepContext.Context.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
                    break;

                case "Attractor":
                    await actionHandler.AttractorActionAsync(stepContext.Context, userProfile, cancellationToken, this);
                    break;
                case "Void":
                    await actionHandler.VoidActionAsync(stepContext.Context, userProfile, cancellationToken, this);
                    break;
                case "Anomaly":
                    await actionHandler.AnomalyActionAsync(stepContext.Context, userProfile, cancellationToken, this);
                    break;
                case "Intent Suggestions":
                    await actionHandler.IntentSuggestionActionAsync(stepContext.Context, userProfile, cancellationToken, this);
                    break;
                case "Pair":
                    await actionHandler.PairActionAsync(stepContext.Context, userProfile, cancellationToken, this);
                    break;
                case "Blind Spots":
                    return await stepContext.BeginDialogAsync(nameof(BlindSpotsDialog), this, cancellationToken);
                case "Scan":
                    return await stepContext.BeginDialogAsync(nameof(ScanDialog), this, cancellationToken);
                case "My Location":
                    repromptThisRound = true;
                    await actionHandler.LocationActionAsync(stepContext.Context, userProfile, cancellationToken);
                    break;
                case "Settings":
                    return await stepContext.BeginDialogAsync(nameof(SettingsDialog), this, cancellationToken);
            }

            if (repromptThisRound)
            {
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
            else
            {
                // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private IList<Choice> GetActionChoices(ITurnContext turnContext)
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "Attractor",
                    Synonyms = new List<string>()
                                    {
                                        "attractor",
                                        "getattractor",
                                    }
                },
                new Choice() {
                    Value = "Void",
                    Synonyms = new List<string>()
                                    {
                                        "void",
                                        "getvoid",
                                        "Repeller",
                                        "repeller",
                                        "getrepeller",
                                    }
                },
                new Choice() {
                    Value = "Anomaly",
                    Synonyms = new List<string>()
                                    {
                                        "anomaly",
                                        "getanomaly",
                                        "ida",
                                        "getida",
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
                                    }
                },
                new Choice() {
                    Value = "Blind Spots",
                    Synonyms = new List<string>()
                                    {
                                        "blind spots",
                                        "blindspots",
                                    }
                },
                new Choice() {
                    Value = "Scan",
                    Synonyms = new List<string>()
                                    {
                                        "scan",
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

            // Hack coz Facebook Messenge stopped showing "Send Location" button
            if (turnContext.Activity.ChannelId.Equals("facebook"))
            {
                actionOptions.Insert(0, new Choice()
                {
                    Value = "Set Location",
                    Synonyms = new List<string>()
                                    {
                                        "Set location",
                                        "set location",
                                        "setlocation"
                                    }
                });
            }

            return actionOptions;
        }
    }
}
