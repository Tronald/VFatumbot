using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace VFatumbot
{
    public class TripReportDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public TripReportDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, ILogger<MainDialog> logger) : base(nameof(TripReportDialog))
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
            _logger.LogInformation("TripReportDialog.ChoiceActionStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("(IN-DEV, not functioning yet) Did you visit this point and would you like to report on your experience?"),
                RetryPrompt = MessageFactory.Text("That is not a valid action."),
                Choices = GetActionChoices(),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            // TODO: implement reporting!!

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "No":
                    return await stepContext.EndDialogAsync();
                    break;
                case "Yes and report":
                    return await stepContext.EndDialogAsync();
                    break;
                case "Yes sans report":
                    return await stepContext.EndDialogAsync();
                    break;
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private IList<Choice> GetActionChoices()
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "No",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
                new Choice() {
                    Value = "Yes and report",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
                new Choice() {
                    Value = "Yes sans report",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
            };

            return actionOptions;
        }
    }
}
