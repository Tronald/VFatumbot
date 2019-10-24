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
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        protected readonly MainDialog _mainDialog;

        public ScanDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, MainDialog mainDialog, ILogger<MainDialog> logger) : base(nameof(ScanDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;
            _mainDialog = mainDialog;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceActionStepAsync,
                PerformActionStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        public async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ScanDialog.ChoiceActionStepAsync");

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
            _logger.LogInformation($"ScanDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var actionHandler = new ActionHandler();
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            var goBackMainMenuThisRound = false;

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Scan Attractor":
                    await actionHandler.AttractorActionAsync(stepContext.Context, userProfile, cancellationToken, _mainDialog, true);
                    break;
                case "Scan Void":
                    await actionHandler.VoidActionAsync(stepContext.Context, userProfile, cancellationToken, _mainDialog, true);
                    break;
                case "Scan Anomaly":
                    await actionHandler.AnomalyActionAsync(stepContext.Context, userProfile, cancellationToken, _mainDialog, true);
                    break;
                case "Scan Pair":
                    await actionHandler.PairActionAsync(stepContext.Context, userProfile, cancellationToken, _mainDialog, true);
                    break;
                case "< Back":
                    goBackMainMenuThisRound = true;
                    break;
            }

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
