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

            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), DiceMaxValidatorAsync)
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                EnterDiceMaxStepAsync,
                RollQDiceStepAsync,
                RollAgainStepAsync
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EnterDiceMaxStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("QuantumDiceDialog.EnterDiceMaxStepAsync");

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Let's roll the quantum dice! Enter a maximum number (inclusive, up to 255):") };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
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
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid maxium, must be less than or equal to 255. Try again:"), cancellationToken);
                return false;
            }

            return true;
        }

        private async Task<DialogTurnResult> RollQDiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"QuantumDiceDialog.RollQDiceStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            int maxValue;
            if (stepContext.Values != null && stepContext.Values.ContainsKey("Result"))
            {
                maxValue = (int)stepContext.Values["Result"];
            }
            else
            {
                maxValue = (int)stepContext.Result;
            }
            var qrng = new QuantumRandomNumberGeneratorWrapper(stepContext.Context, _mainDialog, cancellationToken);
            var diceValue = qrng.Next(0, maxValue);
            stepContext.Values["Result"] = diceValue;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Dice value: {diceValue}. Roll again?"),
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

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Yes":
                    return await RollQDiceStepAsync(stepContext, cancellationToken);
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
        }
    }
}
