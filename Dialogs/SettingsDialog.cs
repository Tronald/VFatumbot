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
    public class SettingsDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public SettingsDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, ILogger<MainDialog> logger) : base(nameof(SettingsDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), RadiusValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CurrentSettingsStepAsync,
                UpdateSettingsYesOrNoStepAsync,
                RadiusStepAsync,
                WaterPointsStepAsync,
                UpdateWaterPointsYesOrNoStepAsync,
                GoogleThumbnailsDisplayToggleStepAsync,
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
                    // TODO: a quick hack to reset IsScanning in case it gets stuck in that state
                    var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
                    userProfile.IsScanning = false;
                    await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);
                    // << EOF TODO. Will figure out whether this needs handling properly later on.

                    return await stepContext.NextAsync();
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken:cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RadiusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.RadiusStepAsync");

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter desired radius in meters:") };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<bool> RadiusValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int inputtedRadius;
            if (!int.TryParse(promptContext.Context.Activity.Text, out inputtedRadius))
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid radius. Enter desired radius:"), cancellationToken);
                return false;
            }

            if (inputtedRadius < Consts.RADIUS_MIN)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Radius must be more than or equal to {Consts.RADIUS_MIN}m. Enter desired radius:"), cancellationToken);
                return false;
            }

            if (inputtedRadius > Consts.RADIUS_MAX)
            {
                await promptContext.Context.SendActivityAsync(MessageFactory.Text($"Radius must be less than or equal to {Consts.RADIUS_MAX}m. Enter desired radius:"), cancellationToken);
                return false;
            }

            return true;
        }

        private async Task<DialogTurnResult> WaterPointsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"SettingsDialog.WaterPointsStepAsync");

            var inputtedRadius = (int)stepContext.Result;
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            userProfile.Radius = inputtedRadius;
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

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Also display Google Street View and Earth thumbnails?"), cancellationToken);
        }

        private async Task<DialogTurnResult> GoogleThumbnailsDisplayToggleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SettingsDialog.UpdateGoogleThumbnailsDisplayToggleStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    userProfile.IsDisplayGoogleThumbnails = true;
                    break;
                case "No":
                default:
                    userProfile.IsDisplayGoogleThumbnails = false;
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

            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
        }

        public async Task ShowCurrentSettingsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                $"Your anonymized ID is {userProfile.UserId}.\n\n" +
                $"Water points will be {(userProfile.IsIncludeWaterPoints ? "included" : "skipped")}.\n\n" +
                $"Street View and Earth thumbnails will be {(userProfile.IsDisplayGoogleThumbnails ? "displayed" : "hidden")}.\n\n" +
                $"Current location is {userProfile.Latitude},{userProfile.Longitude}.\n\n" +
                $"Current radius is {userProfile.Radius}m.\n\n"));
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
