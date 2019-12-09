using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public class ChainsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;
        protected readonly MainDialog _mainDialog;

        public ChainsDialog(IStatePropertyAccessor<UserProfileTemporary> userProfileTemporaryAccessor, MainDialog mainDialog, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(ChainsDialog))
        {
            _logger = logger;
            _userProfileTemporaryAccessor = userProfileTemporaryAccessor;
            _mainDialog = mainDialog;

            TelemetryClient = telemetryClient;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), DistanceValidatorAsync)
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoosePointTypeStepAsync,
                ChooseCenterLocationTypeStepAsync,
                EnterDistanceStepAsync,
                StartChainingStepAsync
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoosePointTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("ChainsDialog.ChoosePointTypeStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What kind of points do you want to chain?"),
                RetryPrompt = MessageFactory.Text("That is not a valid point type."),
                Choices = new List<Choice>()
                {
                    new Choice() {
                        Value = "Attractors",
                        Synonyms = new List<string>()
                                        {
                                            "attractors",
                                        }
                    },
                    new Choice() {
                        Value = "Voids",
                        Synonyms = new List<string>()
                                        {
                                            "voids",
                                            "Repellers",
                                            "repellers",
                                        }
                    },
                    new Choice() {
                        Value = "Anomalies",
                        Synonyms = new List<string>()
                                        {
                                            "anomalies",
                                        }
                    },
                    // TODO: implement one day
                    //new Choice() {
                    //    Value = "Quantums",
                    //    Synonyms = new List<string>()
                    //                    {
                    //                        "quantums",
                    //                    }
                    //},
                    //new Choice() {
                    //    Value = "Pseudos",
                    //    Synonyms = new List<string>()
                    //                    {
                    //                        "pseudos",
                    //                    }
                    //},
                    //new Choice() {
                    //    Value = "Mystery Points",
                    //    Synonyms = new List<string>()
                    //                    {
                    //                        "Mystery points",
                    //                        "mystery points",
                    //                    }
                    //},
                    //new Choice() {
                    //    Value = "< Back",
                    //    Synonyms = new List<string>()
                    //                    {
                    //                        "<",
                    //                        "Back",
                    //                        "back",
                    //                        "<back",
                    //                    }
                    //},
                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ChooseCenterLocationTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"ChainsDialog.ChooseCenterLocationTypeStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            stepContext.Values["point_type"] = ((FoundChoice)stepContext.Result)?.Value;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Generate all points from your current location or in sequential order?"),
                RetryPrompt = MessageFactory.Text("That is not a valid location-generating option."),
                Choices = new List<Choice>()
                {
                    new Choice() {
                        Value = "Current",
                        Synonyms = new List<string>()
                                        {
                                            "current",
                                        }
                    },
                    new Choice() {
                        Value = "Sequential",
                        Synonyms = new List<string>()
                                        {
                                            "sequential",
                                        }
                    },
                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> EnterDistanceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"ChainsDialog.EnterDistanceStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            stepContext.Values["center_location"] = ((FoundChoice)stepContext.Result)?.Value;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Enter number of points to chain. Approximate maximum total distance will be that number multiplied by your current radius."),
                RetryPrompt = MessageFactory.Text("That is not a valid number."),
            };

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), options, cancellationToken);
        }

        private async Task<bool> DistanceValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            float inputtedDistance;
            if (!float.TryParse(promptContext.Context.Activity.Text, out inputtedDistance))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid number. Enter number of points:"), cancellationToken);
                return false;
            }

            if (inputtedDistance < Consts.CHAIN_DISTANCE_MIN)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Number must be more than or equal to {Consts.CHAIN_DISTANCE_MIN}. Enter number:"), cancellationToken);
                return false;
            }

            if (inputtedDistance > Consts.CHAIN_DISTANCE_MAX)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Number must be less than or equal to {Consts.CHAIN_DISTANCE_MAX}. Enter number:"), cancellationToken);
                return false;
            }

            return true;
        }

        private async Task<DialogTurnResult> StartChainingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("ChainsDialog.StartChainingStepAsync");

            stepContext.Values["preferred_distance"] = stepContext.Result;

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(
            //    $"type: {stepContext.Values["point_type"]}\n\n" +
            //    $"center generation: {stepContext.Values["center_location"]}\n\n" +
            //    $"preferred distance: {stepContext.Values["preferred_distance"]}\n\n"
            //    ), cancellationToken);


            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var actionHandler = new ActionHandler();

            switch (stepContext.Values["point_type"].ToString())
            {
                case "Attractors":
                    await actionHandler.ChainActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog,
                                            Enums.PointTypes.Attractor, (int)stepContext.Values["preferred_distance"], stepContext.Values["center_location"].ToString().ToLower().Equals("current"));
                    break;
                case "Voids":
                    await actionHandler.ChainActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog,
                                            Enums.PointTypes.Void, (int)stepContext.Values["preferred_distance"], stepContext.Values["center_location"].ToString().ToLower().Equals("current"));
                    break;
                case "Anomalies":
                    await actionHandler.ChainActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog,
                                            Enums.PointTypes.Anomaly, (int)stepContext.Values["preferred_distance"], stepContext.Values["center_location"].ToString().ToLower().Equals("current"));
                    break;
                case "Quantums":
                    await actionHandler.ChainActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog,
                                            Enums.PointTypes.Quantum, (int)stepContext.Values["preferred_distance"], stepContext.Values["center_location"].ToString().ToLower().Equals("current"));
                    break;
                case "Pseudos":
                    await actionHandler.ChainActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog,
                                            Enums.PointTypes.Pseudo, (int)stepContext.Values["preferred_distance"], stepContext.Values["center_location"].ToString().ToLower().Equals("current"));
                    break;
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
