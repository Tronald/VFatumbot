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
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;

        public SettingsDialog(IStatePropertyAccessor<UserProfileTemporary> userProfileTemporaryAccessor, ILogger<MainDialog> logger) : base(nameof(SettingsDialog))
        {
            _logger = logger;
            _userProfileTemporaryAccessor = userProfileTemporaryAccessor;

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
            //_logger.LogInformation("SettingsDialog.CurrentSettingsStepAsync");

            await ShowCurrentSettingsAsync(stepContext, cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Update your settings?"), cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateSettingsYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"SettingsDialog.UpdateSettingsYesOrNoStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Yes":
                    // TODO: a quick hack to reset IsScanning in case it gets stuck in that state
                    var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context);
                    userProfileTemporary.IsScanning = false;
                    await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);
                    // << EOF TODO. Will figure out whether this needs handling properly later on.

                    return await stepContext.NextAsync();
                case "No":
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken:cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RadiusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("SettingsDialog.RadiusStepAsync");

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
            //_logger.LogInformation($"SettingsDialog.WaterPointsStepAsync");

            var inputtedRadius = (int)stepContext.Result;
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context);
            userProfileTemporary.Radius = inputtedRadius;
            await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Include water points?"), cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateWaterPointsYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("SettingsDialog.UpdateWaterPointsYesOrNoStepAsync");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context);
            await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Yes":
                    userProfileTemporary.IsIncludeWaterPoints = true;
                    break;
                case "No":
                default:
                    userProfileTemporary.IsIncludeWaterPoints = false;
                    break;
            }

            await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), GetPromptOptions("Also display Google Street View and Earth thumbnails?"), cancellationToken);
        }

        private async Task<DialogTurnResult> GoogleThumbnailsDisplayToggleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"SettingsDialog.UpdateGoogleThumbnailsDisplayToggleStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context);
            await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Yes":
                    userProfileTemporary.IsDisplayGoogleThumbnails = true;
                    break;
                case "No":
                default:
                    userProfileTemporary.IsDisplayGoogleThumbnails = false;
                    break;
            }

            await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FinishSettingsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("SettingsDialog.FinishSettingsStepAsync");

            await ShowCurrentSettingsAsync(stepContext, cancellationToken);

            await stepContext.EndDialogAsync(cancellationToken);

            var callbackOptions = new CallbackOptions();
            callbackOptions.UpdateSettings = true;

            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), callbackOptions, cancellationToken);
        }

        public async Task ShowCurrentSettingsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context);
            await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                $"Your anonymized ID is {userProfileTemporary.UserId}.{Helpers.GetNewLine(stepContext.Context)}" +
                $"Water points will be {(userProfileTemporary.IsIncludeWaterPoints ? "included" : "skipped")}.{Helpers.GetNewLine(stepContext.Context)}" +
                $"Street View and Earth thumbnails will be {(userProfileTemporary.IsDisplayGoogleThumbnails ? "displayed" : "hidden")}.{Helpers.GetNewLine(stepContext.Context)}" +
                $"Current location is {userProfileTemporary.Latitude},{userProfileTemporary.Longitude}.{Helpers.GetNewLine(stepContext.Context)}" +
                $"Current radius is {userProfileTemporary.Radius}m.{Helpers.GetNewLine(stepContext.Context)}"));
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
