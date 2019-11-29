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
    public class ScanDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;
        protected readonly MainDialog _mainDialog;

        public ScanDialog(IStatePropertyAccessor<UserProfileTemporary> userProfileTemporaryAccessor, MainDialog mainDialog, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(ScanDialog))
        {
            _logger = logger;
            _userProfileTemporaryAccessor = userProfileTemporaryAccessor;
            _mainDialog = mainDialog;

            TelemetryClient = telemetryClient;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceActionStepAsync,
                PerformActionStepAsync,
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("ScanDialog.ChoiceActionStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose the kind of scan:"),
                RetryPrompt = MessageFactory.Text("That is not a valid scan."),
                Choices = GetActionChoices(),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"ScanDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var actionHandler = new ActionHandler();
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var goBackMainMenuThisRound = false;

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Scan Attractor":
                    if (!userProfileTemporary.IsScanning)
                    {
                        await actionHandler.AttractorActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog, true);
                    }
                    else
                    {
                        goBackMainMenuThisRound = true;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your scanning session is already in progress."), cancellationToken);
                    }
                    break;
                case "Scan Void":
                    if (!userProfileTemporary.IsScanning)
                    {
                        await actionHandler.VoidActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog, true);
                    }
                    else
                    {
                        goBackMainMenuThisRound = true;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your scanning session is already in progress."), cancellationToken);
                    }
                    break;
                case "Scan Anomaly":
                    if (!userProfileTemporary.IsScanning)
                    {
                        await actionHandler.AnomalyActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog, true);
                    }
                    else
                    {
                        goBackMainMenuThisRound = true;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your scanning session is already in progress."), cancellationToken);
                    }
                    break;
                case "Scan Pair":
                    if (!userProfileTemporary.IsScanning)
                    {
                        await actionHandler.PairActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog, true);
                    }
                    else
                    {
                        goBackMainMenuThisRound = true;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your scanning session is already in progress."), cancellationToken);
                    }
                    break;
                case "< Back":
                    goBackMainMenuThisRound = true;
                    break;
            }

            if (goBackMainMenuThisRound)
            {
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
            else
            {
                // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
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
                                    }
                },
                new Choice() {
                    Value = "Scan Void",
                    Synonyms = new List<string>()
                                    {
                                        "scanvoid",
                                    }
                },
                new Choice() {
                    Value = "Scan Anomaly",
                    Synonyms = new List<string>()
                                    {
                                        "scananomaly",
                                    }
                },
                new Choice() {
                    Value = "Scan Pair",
                    Synonyms = new List<string>()
                                    {
                                        "scanpair",
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
