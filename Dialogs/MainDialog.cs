﻿using System;
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
        protected readonly UserPersistentState _userPersistentState;
        protected readonly UserTemporaryState _userTemporaryState;
        protected readonly IStatePropertyAccessor<UserProfilePersistent> _userProfilePersistentAccessor;
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;

        public MainDialog(UserPersistentState userPersistentState, UserTemporaryState userTemporaryState, ConversationState conversationState, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(MainDialog))
        {
            _logger = logger;
            _userPersistentState = userPersistentState;
            _userTemporaryState = userTemporaryState;

            TelemetryClient = telemetryClient;

            if (_userPersistentState != null)
                _userProfilePersistentAccessor = userPersistentState.CreateProperty<UserProfilePersistent>(nameof(UserProfilePersistent));

            if (userTemporaryState != null)
                _userProfileTemporaryAccessor = userTemporaryState.CreateProperty<UserProfileTemporary>(nameof(UserProfileTemporary));

            AddDialog(new MoreStuffDialog(_userProfileTemporaryAccessor, this, logger, telemetryClient));
            AddDialog(new TripReportDialog(_userProfileTemporaryAccessor, this, logger, telemetryClient));
            AddDialog(new ScanDialog(_userProfileTemporaryAccessor, this, logger, telemetryClient));
            AddDialog(new SettingsDialog(_userProfileTemporaryAccessor, logger, telemetryClient));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new ChoicePrompt("AskHowManyIDAsChoicePrompt",
                (PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken) =>
                {
                    // override validater result to also allow free text entry for ratings
                    int idacou;
                    if (int.TryParse(promptContext.Context.Activity.Text, out idacou))
                    {
                        if (idacou < 1 || idacou > 20)
                        {
                            return Task.FromResult(false);
                        }

                        return Task.FromResult(true);
                    }

                    return Task.FromResult(false);
                })
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceActionStepAsync,
                PerformActionStepAsync,
                AskHowManyIDAsStepAsync,
                GetHowManyIDAsStepAsync,
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Helpers.IsRandoLobby(stepContext.Context))
            {
                // Don't spam Randonauts Telegram Lobby with dialog menus as they get sent to everyone
                return await stepContext.EndDialogAsync();
            }

            // Shortcut to trip report dialog testing
            //return await stepContext.ReplaceDialogAsync(nameof(TripReportDialog), new CallbackOptions(), cancellationToken);

            var userProfilePersistent = await _userProfilePersistentAccessor.GetAsync(stepContext.Context, () => new UserProfilePersistent());
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());

            if (stepContext.Options != null)
            {
                // Callback options passed after resuming dialog after long-running background threads etc have finished
                // and resume dialog via the Adapter class's callback method.
                var callbackOptions = (CallbackOptions)stepContext.Options;

                if (callbackOptions.ResetFlag)
                {
                    userProfileTemporary.IsScanning = false;
                    await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary, cancellationToken);
                    await _userTemporaryState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }

                if (callbackOptions.StartTripReportDialog)
                {
                    return await stepContext.ReplaceDialogAsync(nameof(TripReportDialog), callbackOptions, cancellationToken);
                }

                if (callbackOptions.UpdateIntentSuggestions)
                {
                    userProfileTemporary.IntentSuggestions = callbackOptions.IntentSuggestions;
                    userProfileTemporary.TimeIntentSuggestionsSet = callbackOptions.TimeIntentSuggestionsSet;
                    await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary, cancellationToken);
                    await _userTemporaryState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }

                if (callbackOptions.UpdateSettings)
                {
                    userProfilePersistent.IsIncludeWaterPoints = userProfileTemporary.IsIncludeWaterPoints;
                    userProfilePersistent.IsDisplayGoogleThumbnails = userProfileTemporary.IsDisplayGoogleThumbnails;
                    await _userProfilePersistentAccessor.SetAsync(stepContext.Context, userProfilePersistent, cancellationToken);
                    await _userPersistentState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }
            }

            // Make sure the persistent settings are in synch with the temporary ones
            bool doSync = false;
            if (userProfileTemporary.IsIncludeWaterPoints != userProfilePersistent.IsIncludeWaterPoints)
            {
                userProfileTemporary.IsIncludeWaterPoints = userProfilePersistent.IsIncludeWaterPoints;
                doSync = true;
            }
            if (userProfileTemporary.IsDisplayGoogleThumbnails != userProfilePersistent.IsDisplayGoogleThumbnails)
            {
                userProfileTemporary.IsDisplayGoogleThumbnails = userProfilePersistent.IsDisplayGoogleThumbnails;
                doSync = true;
            }
            if (doSync)
            {
                await _userTemporaryState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
            }

            //_logger.LogInformation("MainDialog.ChoiceActionStepAsync");

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
            //_logger.LogInformation($"MainDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var actionHandler = new ActionHandler();
            var repromptThisRound = false;

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                // Hack coz Facebook Messenge stopped showing "Send Location" button
                case "Set Location":
                    repromptThisRound = true;
                    await stepContext.Context.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
                    break;
                case "Attractor":
                    stepContext.Values["PointType"] = "Attractor";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Void":
                    stepContext.Values["PointType"] = "Void";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Anomaly":
                    stepContext.Values["PointType"] = "Anomaly";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Intent Suggestions":
                    await actionHandler.IntentSuggestionActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this);
                    break;
                case "Pair":
                    stepContext.Values["PointType"] = "Pair";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Blind Spots & More":
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(MoreStuffDialog), this, cancellationToken);
                case "Scan":
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(ScanDialog), this, cancellationToken);
                case "My Location":
                    repromptThisRound = true;
                    await actionHandler.LocationActionAsync(stepContext.Context, userProfileTemporary, cancellationToken);
                    break;
                case "Settings":
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
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

        private async Task<DialogTurnResult> AskHowManyIDAsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.AskHowManyIDAsStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Up to how many IDAs (anomalies) to look for? (or enter a number up to 20):"),
                RetryPrompt = MessageFactory.Text("That is not a valid number. It should be a number from 1 to 20."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "1" },
                                    new Choice() { Value = "2" },
                                    new Choice() { Value = "5" },
                                    new Choice() { Value = "10" },
                                }
            };

            return await stepContext.PromptAsync("AskHowManyIDAsChoicePrompt", options, cancellationToken);
        }

        private async Task<DialogTurnResult> GetHowManyIDAsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.GetHowManyIDAsStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var actionHandler = new ActionHandler();

            int idacou;
            if (stepContext.Result == null)
            {
                idacou = int.Parse(stepContext.Context.Activity.Text); // manually inputted a number
            }
            else
            {
                idacou = int.Parse(((FoundChoice)stepContext.Result)?.Value);
            }

            switch (stepContext.Values["PointType"].ToString())
            {
                case "Attractor":
                    await actionHandler.AttractorActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou:idacou);
                    break;
                case "Void":
                    await actionHandler.VoidActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou: idacou);
                    break;
                case "Anomaly":
                    await actionHandler.AnomalyActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou: idacou);
                    break;
                case "Pair":
                    await actionHandler.PairActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou: idacou);
                    break;
            }

            // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
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
                    Value = "Blind Spots & More",
                    Synonyms = new List<string>()
                                    {
                                        "Blind spots & more",
                                        "blind spots & more",
                                        "Blind Spots and More",
                                        "Blind spots and more",
                                        "blind spots and more",
                                        "More stuff",
                                        "more stuff",
                                        "morestuff",
                                        "Blind Spots",
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
