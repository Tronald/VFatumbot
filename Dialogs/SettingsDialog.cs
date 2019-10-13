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

            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CurrentSettingsStepAsync,
                RadiusStepAsync,
                WaterPointsStepAsync,
                FinishSettingsStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CurrentSettingsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            UserProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserProfile());
            await _userState.SaveChangesAsync(stepContext.Context, true, cancellationToken);

            await ShowCurrentSettingsAsync(stepContext, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> RadiusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter desired radius in meters:") };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> WaterPointsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile.Radius = (int)stepContext.Result;
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Include water points? Yes/No") };
            await _userState.SaveChangesAsync(stepContext.Context, true, cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinishSettingsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile.IsIncludeWaterPoints = ((string)stepContext.Result).ToLower().Contains("yes");
            await ShowCurrentSettingsAsync(stepContext, cancellationToken);
            await stepContext.EndDialogAsync(cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
        }

        public async Task ShowCurrentSettingsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hi, your ID is {UserProfile.UserId} and name is {UserProfile.Username}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Water points will be " + (UserProfile.IsIncludeWaterPoints ? "included" : "skipped")), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current location is {UserProfile.Latitude},{UserProfile.Longitude}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current radius is {UserProfile.Radius}m"), cancellationToken);
        }
    }
}
