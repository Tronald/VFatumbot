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
    public class PrivacyAndTermsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfilePersistent> _userProfilePersistentAccessor;

        public PrivacyAndTermsDialog(IStatePropertyAccessor<UserProfilePersistent> userProfilePersistenAccessor, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(PrivacyAndTermsDialog))
        {
            _logger = logger;
            _userProfilePersistentAccessor = userProfilePersistenAccessor;

            TelemetryClient = telemetryClient;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                MustAgreeStepAsync,
                AgreeYesOrNoStepAsync,
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> MustAgreeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("PrivacyAndTermsDialog.MustAgreeStepAsync");

#if RELEASE_PROD
            var help2 = System.IO.File.ReadAllText("help-prod2.txt").Replace("APP_VERSION", Consts.APP_VERSION);
#else
            var help2 = System.IO.File.ReadAllText("help-dev2.txt").Replace("APP_VERSION", Consts.APP_VERSION);
#endif
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(help2), cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Do you agree to the terms of use and privacy policy, and to be a well behaved Randonaut?"), cancellationToken);
        }

        private async Task<DialogTurnResult> AgreeYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"PrivacyAndTermsDialog.AgreeYesOrNoStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "I agree":
                    var userProfilePersistent = await _userProfilePersistentAccessor.GetAsync(stepContext.Context);
                    userProfilePersistent.HasAgreedToToS = true;
                    await _userProfilePersistentAccessor.SetAsync(stepContext.Context, userProfilePersistent);
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
                case "No":
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"That's a shame. So many random adventures are waiting for you."), cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken:cancellationToken);
            }
        }

        private PromptOptions GetPromptOptions(string prompt)
        {
            return new PromptOptions()
            {
                Prompt = MessageFactory.Text(prompt),
                RetryPrompt = MessageFactory.Text($"That is not a valid answer. {prompt}"),
                Choices = new List<Choice>()
                                {
                                    new Choice() {
                                        Value = "No",
                                        Synonyms = new List<string>()
                                                        {
                                                            "no",
                                                        }
                                    },
                                    new Choice() {
                                        Value = "I agree",
                                        Synonyms = new List<string>()
                                                        {
                                                            "i agree",
                                                            "Agree",
                                                            "agree",
                                                            "Yes",
                                                            "yes"
                                                        }
                                    },
                                }
            };
        }
    }
}
