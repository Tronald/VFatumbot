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
    public class MoreStuffDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;
        protected readonly MainDialog _mainDialog;

        public MoreStuffDialog(IStatePropertyAccessor<UserProfileTemporary> userProfileTemporaryAccessor, MainDialog mainDialog, ILogger<MainDialog> logger) : base(nameof(MoreStuffDialog))
        {
            _logger = logger;
            _userProfileTemporaryAccessor = userProfileTemporaryAccessor;
            _mainDialog = mainDialog;

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
            //_logger.LogInformation($"BlindSpotsDialog.ChoiceActionStepAsync[{stepContext.Result}]");

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
            //_logger.LogInformation($"BlindSpotsDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var actionHandler = new ActionHandler();

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Quantum":
                    await actionHandler.QuantumActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog);
                    break;
                case "Quantum Time":
                    await actionHandler.QuantumActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog, true);
                    break;
                case "Pseudo":
                    await actionHandler.PseudoActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog);
                    break;
                case "Mystery Point":
                    await actionHandler.MysteryPointActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog);
                    break;
                case "Today's Randotrip":
                    await actionHandler.RandotripActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, _mainDialog, DateTime.UtcNow.ToString("yyyy-MM-dd"));
                    break;
                case "< Back":
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }

            // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
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
                                    }
                },
                new Choice() {
                    Value = "Quantum Time",
                    Synonyms = new List<string>()
                                    {
                                        "quantumtime",
                                        "getquantumtime",
                                        "qtime",
                                    }
                },
                new Choice() {
                    Value = "Pseudo",
                    Synonyms = new List<string>()
                                    {
                                        "pseudo",
                                        "getpseudo",
                                    }
                },
                new Choice() {
                    Value = "Mystery Point",
                    Synonyms = new List<string>()
                                    {
                                        "Mystery point",
                                        "mystery point",
                                        "Point",
                                        "Point",
                                        "point",
                                        "getpoint",
                                    }
                },
                new Choice() {
                    Value = "Today's Randotrip",
                    Synonyms = new List<string>()
                                    {
                                        "Today's randotrip",
                                        "Todays randotrip",
                                        "today's randotrip",
                                        "todays randotrip",
                                        "randotrip",
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
