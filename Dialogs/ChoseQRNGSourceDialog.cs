using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using VFatumbot.BotLogic;
using static VFatumbot.AdapterWithErrorHandler;

namespace VFatumbot
{
    public class ChoseQRNGSourceDialog : ComponentDialog
    {
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        public extern static int getOptimizedDots(double areaRadiusM); //how many coordinates is needed for requested radius, optimized for performance on larger areas
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int requiredEnthropyBytes(int N); // N* POINT_ENTROPY_BYTES 

        protected readonly ILogger _logger;
        protected readonly UserPersistentState _userPersistentState;
        protected readonly UserTemporaryState _userTemporaryState;
        protected readonly IStatePropertyAccessor<UserProfilePersistent> _userProfilePersistentAccessor;
        protected readonly IStatePropertyAccessor<UserProfileTemporary> _userProfileTemporaryAccessor;

        public ChoseQRNGSourceDialog(UserPersistentState userPersistentState, UserTemporaryState userTemporaryState, ConversationState conversationState, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(MainDialog))
        {
            _logger = logger;
            _userPersistentState = userPersistentState;
            _userTemporaryState = userTemporaryState;

            TelemetryClient = telemetryClient;

            if (_userPersistentState != null)
                _userProfilePersistentAccessor = userPersistentState.CreateProperty<UserProfilePersistent>(nameof(UserProfilePersistent));

            if (userTemporaryState != null)
                _userProfileTemporaryAccessor = userTemporaryState.CreateProperty<UserProfileTemporary>(nameof(UserProfileTemporary));

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new TextPrompt("GetQRNGSourceChoicePrompt",
                (PromptValidatorContext<string> promptContext, CancellationToken cancellationToken) =>
                {
                    // verify it's a 64 char hex string (sha256 of the entropy generated)
                    if (promptContext.Context.Activity.Text.Length != 64)
                    {
                        return Task.FromResult(false);
                    }

                    // regex check
                    Regex regex = new Regex("^[a-fA-F0-9]+$");
                    return Task.FromResult(regex.IsMatch(promptContext.Context.Activity.Text));
                })
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SelectQRNGSourceStepAsync,
                GetQRNGSourceStepAsync,
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectQRNGSourceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.SelectQRNGSourceStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            // Number of IDAs to look for from previous step
            if (stepContext.Result == null)
            {
                stepContext.Values["idacou"] = int.Parse(stepContext.Context.Activity.Text); // manually inputted a number
            }
            else
            {
                stepContext.Values["idacou"] = int.Parse(((FoundChoice)stepContext.Result)?.Value);
            }

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            if (userProfileTemporary.BotSrc == Enums.WebSrc.ios || userProfileTemporary.BotSrc == Enums.WebSrc.android)
            {
                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Chose your source of entropy for the quantum random number generator:"),
                    RetryPrompt = MessageFactory.Text("That is not a valid QRNG source."),
                    Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Camera" },
                                    //new Choice() { Value = "GCP" },
                                    new Choice() { Value = "ANU" },
                                }
                };

                return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> GetQRNGSourceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.GetQRNGSourceStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            if (userProfileTemporary.BotSrc == Enums.WebSrc.ios || userProfileTemporary.BotSrc == Enums.WebSrc.android)
            {
                stepContext.Values["qrng_source"] = "ANU";
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Camera":
                    stepContext.Values["qrng_source"] = "Camera";

                    var promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Your smartphone's camera will now load to generate some local entropy to send to me."),
                        RetryPrompt = MessageFactory.Text("That is not a valid entropy source."),
                    };

                    // Get the number of bytes we need from the camera's entropy
                    int numDots = getOptimizedDots(userProfileTemporary.Radius);
                    int bytesSize = requiredEnthropyBytes(numDots);

                    // Send an EventActivity to for the webbot's JavaScript callback handler to pickup
                    // and then pass onto the app layer to load the camera
                    var requestEntropyActivity = Activity.CreateEventActivity();
                    requestEntropyActivity.ChannelData = $"camrng,{bytesSize}";
                    await stepContext.Context.SendActivityAsync(requestEntropyActivity);

                    return await stepContext.PromptAsync("GetQRNGSourceChoicePrompt", promptOptions, cancellationToken);

                case "GCP":
                    stepContext.Values["qrng_source"] = "GCP";
                    // TODO: call GCP logic here... if it can be done
                    // Reference: http://gcpdot.com/gcpindex.php?small=100
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                default:
                case "ANU":
                    stepContext.Values["qrng_source"] = "ANU";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
