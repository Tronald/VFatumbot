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
    public class BlindSpotsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        protected readonly MainDialog _mainDialog;

        public BlindSpotsDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, MainDialog mainDialog, ILogger<MainDialog> logger) : base(nameof(BlindSpotsDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;

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
            _logger.LogInformation($"BlindSpotsDialog.ChoiceActionStepAsync[{stepContext.Result}]");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose the kind of blind spot:"),
                RetryPrompt = MessageFactory.Text("That is not a valid blind spot."),
                Choices = GetActionChoices(),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"BlindSpotsDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var actionHandler = new ActionHandler();
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Quantum":
                    await actionHandler.QuantumActionAsync(stepContext, userProfile, cancellationToken);
                    break;
                case "Quantum Time":
                    await actionHandler.QuantumActionAsync(stepContext, userProfile, cancellationToken, true);
                    break;
                case "Psuedo":
                    await actionHandler.PsuedoActionAsync(stepContext, userProfile, cancellationToken);
                    break;
                case "Point":
                    await actionHandler.PointActionAsync(stepContext, userProfile, cancellationToken, _mainDialog);
                    break;
                case "< Back":
                    break;
            }

            return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
        }

        private IList<Choice> GetActionChoices()
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "Quantum",
                    Synonyms = new List<string>()
                                    {
                                        "quantum",
                                        "getquantum",
                                        "/getquantum",
                                    }
                },
                new Choice() {
                    Value = "Quantum Time",
                    Synonyms = new List<string>()
                                    {
                                        "quantumtime",
                                        "getquantumtime",
                                        "qtime",
                                        "/getqtime",
                                    }
                },
                new Choice() {
                    Value = "Psuedo",
                    Synonyms = new List<string>()
                                    {
                                        "psuedo",
                                        "getpsuedo",
                                        "/getpsuedo",
                                    }
                },
                new Choice() {
                    Value = "Point",
                    Synonyms = new List<string>()
                                    {
                                        "point",
                                        "getpoint",
                                        "/getpoint",
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
