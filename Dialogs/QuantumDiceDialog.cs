using System;
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
    public class QuantumDiceDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;
        protected readonly MainDialog _mainDialog;

        public QuantumDiceDialog(IStatePropertyAccessor<UserProfileTemporary> userProfileTemporaryAccessor, MainDialog mainDialog, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(QuantumDiceDialog))
        {
            _logger = logger;
            _userProfileTemporaryAccessor = userProfileTemporaryAccessor;
            _mainDialog = mainDialog;

            TelemetryClient = telemetryClient;

            AddDialog(new NumberPrompt<int>("MinNumberPrompt", DiceMinValidatorAsync)
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new NumberPrompt<int>("MaxNumberPrompt", DiceMaxValidatorAsync)
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                EnterDiceMinStepAsync,
                EnterDiceMaxStepAsync,
                RollQDiceStepAsync,
                RollAgainStepAsync
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EnterDiceMinStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("QuantumDiceDialog.EnterDiceMinStepAsync");


            int alreadyGotMin = -1;
            if (stepContext.Options != null)
            {
                try
                {
                    var minmax = (IDictionary<string, string>)stepContext.Options;
                    if (int.TryParse(minmax["Min"], out alreadyGotMin))
                    {
                        // Rolling again
                        stepContext.Values["Min"] = alreadyGotMin;
                        return await stepContext.NextAsync(cancellationToken: cancellationToken);
                    }
                } catch (Exception) { /* cast exception, bad hack, TLDR: fix later */ }
            }

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter an inclusive minimum number greater than or equal to 1. For a coin toss enter 1 and assign heads to it:") };
            return await stepContext.PromptAsync("MinNumberPrompt", promptOptions, cancellationToken);
        }

        private async Task<bool> DiceMinValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int inputtedDiceMin;
            if (!int.TryParse(promptContext.Context.Activity.Text, out inputtedDiceMin))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid minimum. Enter desired minimum:"), cancellationToken);
                return false;
            }

            if (inputtedDiceMin < 1)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Minimum must greater than or equal to 1. Try again:"), cancellationToken);
                return false;
            }

            if (inputtedDiceMin > 254)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid minimum, must be less than or equal to 254. Try again:"), cancellationToken);
                return false;
            }

            return true;
        }

        private async Task<DialogTurnResult> EnterDiceMaxStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("QuantumDiceDialog.EnterDiceMaxStepAsync");

            int minValue;
            if (stepContext.Values != null && stepContext.Values.ContainsKey("Min"))
            {
                minValue = int.Parse(stepContext.Values["Min"].ToString());
            }
            else
            {
                minValue = (int)stepContext.Result;
            }
            stepContext.Values["Min"] = minValue;

            int alreadyGotMax = -1;
            if (stepContext.Options != null)
            {
                try
                {
                    var minmax = (IDictionary<string, string>)stepContext.Options;
                    if (int.TryParse(minmax["Max"], out alreadyGotMax))
                    {
                        // Rolling again
                        stepContext.Values["Max"] = alreadyGotMax;
                        return await stepContext.NextAsync(cancellationToken: cancellationToken);
                    }
                } catch (Exception) { /* cast exception, bad hack, TLDR: fix later */ }
            }

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter an inclusive maximum number greater than the minimum, up to 255. For a coin toss enter 2 and assign tails to it:") };
            return await stepContext.PromptAsync("MaxNumberPrompt", promptOptions, cancellationToken);
        }

        private async Task<bool> DiceMaxValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int inputtedDiceMax;
            if (!int.TryParse(promptContext.Context.Activity.Text, out inputtedDiceMax))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid maximum. Enter desired maximum:"), cancellationToken);
                return false;
            }

            if (inputtedDiceMax <= 1)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Maximum must greater than 1. Try again:"), cancellationToken);
                return false;
            }

            if (inputtedDiceMax > 0xff)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid maximum, must be less than or equal to 255. Try again:"), cancellationToken);
                return false;
            }

            return true;
        }

        private async Task<DialogTurnResult> RollQDiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"QuantumDiceDialog.RollQDiceStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            int minValue = int.Parse(stepContext.Values["Min"].ToString());

            int maxValue;
            if (stepContext.Values != null && stepContext.Values.ContainsKey("Max"))
            {
                maxValue = int.Parse(stepContext.Values["Max"].ToString());
            }
            else
            {
                maxValue = (int)stepContext.Result;
            }

            if (minValue > maxValue)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Minimum ({minValue}) must be less than the maximum ({maxValue}). Setting it to {minValue-1}."), cancellationToken);
                minValue--;
                stepContext.Values["Min"] = minValue;
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Rolling..."), cancellationToken);

            var qrng = new QuantumRandomNumberGeneratorWrapper(stepContext.Context, _mainDialog, cancellationToken);
            var diceValue = qrng.Next(minValue, maxValue + 1);
            stepContext.Values["Max"] = maxValue;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Result: {diceValue} [{minValue},{maxValue}]. Roll again?"),
                RetryPrompt = MessageFactory.Text("Yes or No, nothing else."),
                Choices = new List<Choice>()
                {
                    new Choice() {
                        Value = "Yes",
                        Synonyms = new List<string>()
                                        {
                                            "yes",
                                        }
                    },
                    new Choice() {
                        Value = "No",
                        Synonyms = new List<string>()
                                        {
                                            "no",
                                        }
                    },
                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RollAgainStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"QuantumDiceDialog.RollAgainStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            IDictionary<string, string> minmax = new Dictionary<string, string>
            {
                { "Min", stepContext.Values["Min"].ToString() },
                { "Max", stepContext.Values["Max"].ToString() }
            };

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Yes":
                    return await stepContext.ReplaceDialogAsync(nameof(QuantumDiceDialog), options: minmax, cancellationToken: cancellationToken);
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
        }
    }
}
