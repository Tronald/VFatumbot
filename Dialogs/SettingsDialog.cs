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
    public class SettingsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly UserState _userState;

        private MainDialog _mainDialog;

        protected UserProfile UserProfile;

        public SettingsDialog(UserState userState, object mainDialog, ILogger<MainDialog> logger) : base(nameof(SettingsDialog))
        {
            _logger = logger;
            _userState = userState;
            _mainDialog = (MainDialog)mainDialog;

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
            UserProfile = _mainDialog.UserProfile;

            await ShowCurrentSettingsAsync(stepContext, cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Update your settings?"), cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateSettingsYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.UpdateSettingsYesOrNoStepAsync");

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    return await stepContext.NextAsync();
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> RadiusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter desired radius in meters:") };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> WaterPointsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile.Radius = (int)stepContext.Result;
            await _userState.SaveChangesAsync(stepContext.Context, true, cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Include water points?"), cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateWaterPointsYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.UpdateWaterPointsYesOrNoStepAsync");

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    UserProfile.IsIncludeWaterPoints = true;
                    break;
                case "No":
                default:
                    UserProfile.IsIncludeWaterPoints = false;
                    break;
            }

            await _userState.SaveChangesAsync(stepContext.Context, true, cancellationToken);

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FinishSettingsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await ShowCurrentSettingsAsync(stepContext, cancellationToken);
            await stepContext.EndDialogAsync(cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
        }

        public async Task ShowCurrentSettingsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"ID is {UserProfile.UserId} and name is {UserProfile.Username}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Water points will be " + (UserProfile.IsIncludeWaterPoints ? "included" : "skipped")), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current location is {UserProfile.Latitude},{UserProfile.Longitude}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current radius is {UserProfile.Radius}m"), cancellationToken);
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
