using System;
using System.Collections.Generic;
using System.Net;
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
    public class MainDialog : ComponentDialog
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

        public MainDialog(UserPersistentState userPersistentState, UserTemporaryState userTemporaryState, ConversationState conversationState, ILogger<MainDialog> logger, IBotTelemetryClient telemetryClient) : base(nameof(MainDialog))
        {
            _logger = logger;
            _userPersistentState = userPersistentState;
            _userTemporaryState = userTemporaryState;

            TelemetryClient = telemetryClient;

            if (_userPersistentState != null)
                _userProfilePersistentAccessor = userPersistentState.CreateProperty<UserProfilePersistent>(nameof(UserProfilePersistent));

            if (userTemporaryState != null)
                _userProfileTemporaryAccessor = userTemporaryState.CreateProperty<UserProfileTemporary>(nameof(UserProfileTemporary));

            AddDialog(new PrivacyAndTermsDialog(_userProfilePersistentAccessor, logger, telemetryClient));
            AddDialog(new MoreStuffDialog(_userProfileTemporaryAccessor, this, logger, telemetryClient));
            AddDialog(new TripReportDialog(_userProfileTemporaryAccessor, this, logger, telemetryClient));
            AddDialog(new ScanDialog(_userProfileTemporaryAccessor, this, logger, telemetryClient));
            AddDialog(new SettingsDialog(_userProfileTemporaryAccessor, logger, telemetryClient));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                TelemetryClient = telemetryClient,
            });
            AddDialog(new ChoicePrompt("AskHowManyIDAsChoicePrompt",
                (PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken) =>
                {
                    // override validater result to also allow free text entry for ratings
                    int idacou;
                    if (int.TryParse(promptContext.Context.Activity.Text, out idacou))
                    {
                        if (idacou < 1 || idacou > 20)
                        {
                            return Task.FromResult(false);
                        }

                        return Task.FromResult(true);
                    }

                    return Task.FromResult(false);
                })
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
                ChoiceActionStepAsync,
                PerformActionStepAsync,
                AskHowManyIDAsStepAsync,
                SelectQRNGSourceStepAsync,
                GetQRNGSourceStepAsync,
                GenerateIDAsStepAsync
            })
            {
                TelemetryClient = telemetryClient,
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoiceActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Helpers.IsRandoLobby(stepContext.Context))
            {
                // Don't spam Randonauts Telegram Lobby with dialog menus as they get sent to everyone
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            // Shortcut to trip report dialog testing
            //return await stepContext.ReplaceDialogAsync(nameof(TripReportDialog), new CallbackOptions(), cancellationToken);

            var userProfilePersistent = await _userProfilePersistentAccessor.GetAsync(stepContext.Context, () => new UserProfilePersistent());
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());

            // Must agree to Privacy Policy and Terms of Service before using
            if (!userProfilePersistent.HasAgreedToToS)
            {
                await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(PrivacyAndTermsDialog), this, cancellationToken);
            }

            if (stepContext.Options != null)
            {
                // Callback options passed after resuming dialog after long-running background threads etc have finished
                // and resume dialog via the Adapter class's callback method.
                var callbackOptions = (CallbackOptions)stepContext.Options;

                if (callbackOptions.ResetFlag)
                {
                    userProfileTemporary.IsScanning = false;
                    await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary, cancellationToken);
                    await _userTemporaryState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }

                if (callbackOptions.StartTripReportDialog)
                {
                    return await stepContext.ReplaceDialogAsync(nameof(TripReportDialog), callbackOptions, cancellationToken);
                }

                if (callbackOptions.UpdateIntentSuggestions)
                {
                    userProfileTemporary.IntentSuggestions = callbackOptions.IntentSuggestions;
                    userProfileTemporary.TimeIntentSuggestionsSet = callbackOptions.TimeIntentSuggestionsSet;
                    await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary, cancellationToken);
                    await _userTemporaryState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }

                if (callbackOptions.UpdateSettings)
                {
                    userProfilePersistent.IsIncludeWaterPoints = userProfileTemporary.IsIncludeWaterPoints;
                    userProfilePersistent.IsDisplayGoogleThumbnails = userProfileTemporary.IsDisplayGoogleThumbnails;
                    await _userProfilePersistentAccessor.SetAsync(stepContext.Context, userProfilePersistent, cancellationToken);
                    await _userPersistentState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
                }
            }

            // Make sure the persistent settings are in synch with the temporary ones
            bool doSync = false;
            if (userProfileTemporary.IsIncludeWaterPoints != userProfilePersistent.IsIncludeWaterPoints)
            {
                userProfileTemporary.IsIncludeWaterPoints = userProfilePersistent.IsIncludeWaterPoints;
                doSync = true;
            }
            if (userProfileTemporary.IsDisplayGoogleThumbnails != userProfilePersistent.IsDisplayGoogleThumbnails)
            {
                userProfileTemporary.IsDisplayGoogleThumbnails = userProfilePersistent.IsDisplayGoogleThumbnails;
                doSync = true;
            }
            if (doSync)
            {
                await _userTemporaryState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
            }

            //_logger.LogInformation("MainDialog.ChoiceActionStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What would you like to get?"),
                RetryPrompt = MessageFactory.Text("That is not a valid action. What would you like to get?"),
                Choices = GetActionChoices(stepContext.Context),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> PerformActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.PerformActionStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");

            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var actionHandler = new ActionHandler();
            var repromptThisRound = false;

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                // Hack coz Facebook Messenge stopped showing "Send Location" button
                case "Set Location":
                    repromptThisRound = true;
                    await stepContext.Context.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
                    break;
                case "Attractor":
                    stepContext.Values["PointType"] = "Attractor";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Void":
                    stepContext.Values["PointType"] = "Void";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Anomaly":
                    stepContext.Values["PointType"] = "Anomaly";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Intent Suggestions":
                    await actionHandler.IntentSuggestionActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this);
                    break;
                case "Pair":
                    stepContext.Values["PointType"] = "Pair";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Blind Spots & More":
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(MoreStuffDialog), this, cancellationToken);
                case "Scan":
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(ScanDialog), this, cancellationToken);
                case "My Location":
                    repromptThisRound = true;
                    await actionHandler.LocationActionAsync(stepContext.Context, userProfileTemporary, cancellationToken);
                    break;
                case "Settings":
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(SettingsDialog), this, cancellationToken);
            }

            if (repromptThisRound)
            {
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
            else
            {
                // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AskHowManyIDAsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.AskHowManyIDAsStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Up to how many IDAs (anomalies) to look for? (or enter a number up to 20):"),
                RetryPrompt = MessageFactory.Text("That is not a valid number. It should be a number from 1 to 20."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "1" },
                                    new Choice() { Value = "2" },
                                    new Choice() { Value = "5" },
                                    new Choice() { Value = "10" },
                                }
            };

            return await stepContext.PromptAsync("AskHowManyIDAsChoicePrompt", options, cancellationToken);
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
                                    new Choice() { Value = "ANU" },
                                    new Choice() { Value = "ANU Leftovers)" },
                                    new Choice() { Value = "GCP Retro" },
                                }
                };

                return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
            }
            else
            {
                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Chose your entropy source:"),
                    RetryPrompt = MessageFactory.Text("That is not a valid entropy source."),
                    Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "ANU" },
                                    new Choice() { Value = "ANU Leftovers" },
                                    new Choice() { Value = "GCP Retro" },
                                }
                };

                return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
            }
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


            // Get the number of bytes we need from the camera's entropy
            int numDots = getOptimizedDots(userProfileTemporary.Radius);
            int bytesSize = requiredEnthropyBytes(numDots);

            switch (((FoundChoice)stepContext.Result)?.Value)
            {
                case "Camera":
                    stepContext.Values["qrng_source"] = "Camera";

                    var promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Your smartphone's camera will now load to generate some local entropy to send to me."),
                        RetryPrompt = MessageFactory.Text("That is not a valid entropy source."),
                    };

                    // Send an EventActivity to for the webbot's JavaScript callback handler to pickup
                    // and then pass onto the app layer to load the camera
                    var requestEntropyActivity = Activity.CreateEventActivity();
                    requestEntropyActivity.ChannelData = $"camrng,{bytesSize}";
                    await stepContext.Context.SendActivityAsync(requestEntropyActivity);

                    return await stepContext.PromptAsync("GetQRNGSourceChoicePrompt", promptOptions, cancellationToken);

                case "ANU Leftovers":
                    stepContext.Values["qrng_source"] = "Pool";

                    // Chose a random entropy GID from the list of GIDs in the pool (pseudo randomly selecting quantum randomness?! there's a joke in there somewhere :)
#if RELEASE_PROD
                    var jsonStr = new WebClient().DownloadString($"https://api.randonauts.com/getpools");
#else
                    var jsonStr = new WebClient().DownloadString($"https://devapi.randonauts.com/getpools");
#endif
                    var pools = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonStr);
                    getpool: // random pool selection (pseudo)
                        var r = new Random();
                        var idx = r.Next(pools.Count);
                        var pool = pools[idx];
                        if (pool.pool.ToString().Equals("0.pool")) goto getpool;
                        var time = DateTime.Parse(pool.time.ToString());

                    await stepContext.Context.SendActivityAsync($"Enjoy some residual entropy from around {time.ToString("yyyy-MM-dd")}");

                    stepContext.Values["qrng_source_query_str"] = $"pool=true&gid={pool.pool.ToString().Replace(".pool", "")}&raw=true";
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                case "GCP Retro":
                    stepContext.Values["qrng_source"] = "GCPRetro";
                    stepContext.Values["qrng_source_query_str"] = $"gcp=true&size={bytesSize * 2}";

                    // Until the libwrapper supports proper paging spanning over multiple days restrict the amount of entropy we ask for to within 5km
                    if (userProfileTemporary.Radius > 5000)
                    {
                        userProfileTemporary.Radius = 5000;
                        await _userProfileTemporaryAccessor.SetAsync(stepContext.Context, userProfileTemporary);
                    }

                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                default:
                case "ANU":
                    stepContext.Values["qrng_source"] = "ANU";
                    stepContext.Values["qrng_source_query_str"] = ""; // generated later in QRNG class
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GenerateIDAsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"MainDialog.SelectQRNGSourceStepAsync[{((FoundChoice)stepContext.Result)?.Value}]");
            var userProfileTemporary = await _userProfileTemporaryAccessor.GetAsync(stepContext.Context, () => new UserProfileTemporary());
            var actionHandler = new ActionHandler();

            var idacou = int.Parse(stepContext.Values["idacou"].ToString());
            string entropyQueryString = stepContext.Context.Activity.Text;

            if (entropyQueryString.Length == 64)
            {
                // Assume 64 chars exactly is entropy GID direct from camera or copy/pasted shared
                stepContext.Values["qrng_source_query_str"] = $"gid={entropyQueryString}&raw=true";
            }
            else
            {
                // GCP Retro / ANU Leftovers (pool)
                entropyQueryString = stepContext.Values["qrng_source_query_str"].ToString();
            }

            switch (stepContext.Values["PointType"].ToString())
            {
                case "Attractor":
                    await actionHandler.AttractorActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou:idacou, entropyQueryString: entropyQueryString);
                    break;
                case "Void":
                    await actionHandler.VoidActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou: idacou, entropyQueryString: entropyQueryString);
                    break;
                case "Anomaly":
                    await actionHandler.AnomalyActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou: idacou, entropyQueryString: entropyQueryString);
                    break;
                case "Pair":
                    await actionHandler.PairActionAsync(stepContext.Context, userProfileTemporary, cancellationToken, this, idacou: idacou, entropyQueryString: entropyQueryString);
                    break;
            }

            // Long-running tasks like /getattractors etc will make use of ContinueDialog to re-prompt users
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private IList<Choice> GetActionChoices(ITurnContext turnContext)
        {
            var actionOptions = new List<Choice>()
            {
                new Choice() {
                    Value = "Attractor",
                    Synonyms = new List<string>()
                                    {
                                        "attractor",
                                        "getattractor",
                                    }
                },
                new Choice() {
                    Value = "Void",
                    Synonyms = new List<string>()
                                    {
                                        "void",
                                        "getvoid",
                                        "Repeller",
                                        "repeller",
                                        "getrepeller",
                                    }
                },
                new Choice() {
                    Value = "Anomaly",
                    Synonyms = new List<string>()
                                    {
                                        "anomaly",
                                        "getanomaly",
                                        "ida",
                                        "getida",
                                    }
                },
                new Choice() {
                    Value = "Intent Suggestions",
                    Synonyms = new List<string>()
                                    {
                                    }
                },
                new Choice() {
                    Value = "Pair",
                    Synonyms = new List<string>()
                                    {
                                        "pair",
                                        "getpair",
                                    }
                },
                new Choice() {
                    Value = "Blind Spots & More",
                    Synonyms = new List<string>()
                                    {
                                        "Blind spots & more",
                                        "blind spots & more",
                                        "Blind Spots and More",
                                        "Blind spots and more",
                                        "blind spots and more",
                                        "More stuff",
                                        "more stuff",
                                        "morestuff",
                                        "Blind Spots",
                                        "blind spots",
                                        "blindspots",
                                    }
                },
                new Choice() {
                    Value = "Scan",
                    Synonyms = new List<string>()
                                    {
                                        "scan",
                                    }
                },
                new Choice() {
                    Value = "My Location",
                    Synonyms = new List<string>()
                                    {
                                        "My Location",
                                        "My location",
                                        "my location",
                                        "location",
                                    }
                },
                 new Choice() {
                    Value = "Settings",
                    Synonyms = new List<string>()
                                    {
                                        "settings",
                                    }
                },
            };

            // Hack coz Facebook Messenge stopped showing "Send Location" button
            if (turnContext.Activity.ChannelId.Equals("facebook"))
            {
                actionOptions.Insert(0, new Choice()
                {
                    Value = "Set Location",
                    Synonyms = new List<string>()
                                    {
                                        "Set location",
                                        "set location",
                                        "setlocation"
                                    }
                });
            }

            return actionOptions;
        }
    }
}
