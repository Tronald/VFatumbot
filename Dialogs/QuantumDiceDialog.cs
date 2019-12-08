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


            int alreadyGotMax = -1;
            if (stepContext.Options != null && int.TryParse(stepContext.Options.ToString(), out alreadyGotMax))
            {
                // Rolling again
                stepContext.Values["Max"] = alreadyGotMax;
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter an inclusive maximum number greater than 1, up to 255. For a simple coin toss, enter 2 and assign heads to 1 and tails to 2:") };
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
            if (stepContext.Values != null && stepContext.Values.ContainsKey("Max"))
            {
                maxValue = int.Parse(stepContext.Values["Max"].ToString());
            }
            else
            {
                maxValue = (int)stepContext.Result;
            }
            var qrng = new QuantumRandomNumberGeneratorWrapper(stepContext.Context, _mainDialog, cancellationToken);
            var diceValue = qrng.Next(maxValue == 1 ? 0 : 1, maxValue + 1);
            stepContext.Values["Max"] = maxValue;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Result: {diceValue}/{maxValue}. Roll again?"),
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
                    return await stepContext.ReplaceDialogAsync(nameof(QuantumDiceDialog), options: stepContext.Values["Max"], cancellationToken: cancellationToken);
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
        }
    }
}
