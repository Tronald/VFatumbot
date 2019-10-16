using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;

namespace VFatumbot
{
    public class SettingsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public SettingsDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, ILogger<MainDialog> logger) : base(nameof(SettingsDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CurrentSettingsStepAsync,
                UpdateSettingsYesOrNoStepAsync,
                RadiusStepAsync,
                WaterPointsStepAsync,
                UpdateWaterPointsYesOrNoStepAsync,
                FinishSettingsStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CurrentSettingsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.CurrentSettingsStepAsync");

            await ShowCurrentSettingsAsync(stepContext, cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Update your settings?"), cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateSettingsYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"SettingsDialog.UpdateSettingsYesOrNoStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    return await stepContext.NextAsync();
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RadiusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.RadiusStepAsync");

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter desired radius in meters:") };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> WaterPointsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"SettingsDialog.WaterPointsStepAsync");

            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            userProfile.Radius = (int)stepContext.Result;
            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Include water points?"), cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateWaterPointsYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.UpdateWaterPointsYesOrNoStepAsync");

            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    userProfile.IsIncludeWaterPoints = true;
                    break;
                case "No":
                default:
                    userProfile.IsIncludeWaterPoints = false;
                    break;
            }

            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FinishSettingsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.FinishSettingsStepAsync");

            await ShowCurrentSettingsAsync(stepContext, cancellationToken);

            await stepContext.EndDialogAsync(cancellationToken);

            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
        }

        public async Task ShowCurrentSettingsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"ID is {userProfile.UserId} and name is {userProfile.Username}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Water points will be " + (userProfile.IsIncludeWaterPoints ? "included" : "skipped")), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current location is {userProfile.Latitude},{userProfile.Longitude}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current radius is {userProfile.Radius}m"), cancellationToken);
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
        }
    }
}
