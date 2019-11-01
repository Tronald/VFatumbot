using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using VFatumbot.BotLogic;
using static VFatumbot.AdapterWithErrorHandler;
using static VFatumbot.BotLogic.Enums;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot
{
    public class TripReportDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        protected readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        protected readonly MainDialog _mainDialog;

        private const string ReportAnswersKey = "value-ReportAnswers";

        public class ReportAnswers {
            public bool WasPointVisited { get; set; }
            public int PointNumberVisited { get; set; }

            public bool SkipGetIntentStep { get; set; }
            public string Intent { get; set; }

            public bool ArtifactCollected { get; set; }

            public bool WasFuckingAmazing { get; set; }

            public string Rating_Meaningfulness { get; set; }
            public string Rating_Emotional { get; set; }
            public string Rating_Importance { get; set; }
            public string Rating_Strangeness { get; set; }
            public string Rating_Synchronicty { get; set; }

            public string Report { get; set; }
            public string[] PhotoURLs { get; set; }
        }

        public TripReportDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, MainDialog mainDialog, ILogger<MainDialog> logger) : base(nameof(TripReportDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;
            _mainDialog = mainDialog;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), FreetextRatingValidator));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ReportYesOrNoStepAsync,
                StartReportStepAsync,
                SetIntentYesOrNoStepAsync,
                GetIntentStepAsync,
                ArtifactsCollectedYesOrNoStepAsync,
                FuckingAmazingYesOrNoStepAsync,
                RateMeaningfulnessStepAsync,
                RateEmotionalStepAsync,
                RateImportanceStepAsync,
                RateStrangenessStepAsync,
                RateSynchronictyStepAsync,
                WriteReportStepAsync,
                FinishStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<bool> FreetextRatingValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return true;
        }

        private async Task<DialogTurnResult> ReportYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("TripReportDialog.ReportYesOrNoStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Did you visit this point and would you like to make a trip report?"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                            {
                                new Choice() {
                                    Value = "No",
                                    Synonyms = new List<string>()
                                                    {
                                                    }
                                },
                                new Choice() {
                                    Value = "Yes and report!",
                                    Synonyms = new List<string>()
                                                    {
                                                    }
                                },
                                new Choice() {
                                    Value = "Yes sans reporting",
                                    Synonyms = new List<string>()
                                                    {
                                                    }
                                }
                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> StartReportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.StartReportStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var callbackOptions = (CallbackOptions)stepContext.Options;

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes and report!":
                    // Go and start asking them about their trip

                    var answers = new ReportAnswers() { WasPointVisited = true };
                    stepContext.Values[ReportAnswersKey] = answers;

                    // TODO: [answers.PointNumberVisited] : implement the dialog steps/logic to ask this.

                    switch (callbackOptions.PointTypes[answers.PointNumberVisited])
                    {
                        case PointTypes.Attractor:
                        case PointTypes.Void:
                        case PointTypes.Anomaly:
                        case PointTypes.PairAttractor:
                        case PointTypes.PairVoid:
                        case PointTypes.ScanAttractor:
                        case PointTypes.ScanVoid:
                        case PointTypes.ScanAnomaly:
                        case PointTypes.ScanPair:
                            var options = new PromptOptions()
                            {
                                Prompt = MessageFactory.Text("Did you set an intent?"),
                                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Yes" },
                                    new Choice() { Value = "No" }
                                }
                            };

                            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
                    }

                    ((ReportAnswers)stepContext.Values[ReportAnswersKey]).SkipGetIntentStep = true;
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);

                case "Yes sans reporting":
                    await stepContext.Context.SendActivityAsync("Hope you had a fun trip!");

                    // At least mark the point as a visited one
                    await StoreReportInDB(stepContext.Context, callbackOptions, new ReportAnswers() { WasPointVisited = true });

                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);

                case "No":
                default:
                    await StoreReportInDB(stepContext.Context, callbackOptions, new ReportAnswers());
                    await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }

        }

        private async Task<DialogTurnResult> SetIntentYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            if (answers.SkipGetIntentStep)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

            _logger.LogInformation($"TripReportDialog.SetIntentYesOrNoStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What did you set your intent to?") };
                    return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

                case "No":
                default:
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetIntentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.GetIntentStepAsync");

            if (!string.IsNullOrEmpty(""+stepContext.Result))
            {
                // Assume they selected "No" for no intent set and skip
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];
            answers.Intent = (string)stepContext.Result;

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ArtifactsCollectedYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.ArtifactsCollectedYesOrNoStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Did you collect any artifacts?"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Yes" },
                                    new Choice() { Value = "No" }
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> FuckingAmazingYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.FuckingAmazingYesOrNoStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    answers.ArtifactCollected = true;
                    break;
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Was your trip \"wow and astounding!\"?"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Yes" },
                                    new Choice() { Value = "No" }
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateMeaningfulnessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateMeaningfulnessStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    answers.WasFuckingAmazing = true;
                    break;
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the meaningfulness of your trip (or enter your own word):"),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Enriching" },
                                    new Choice() { Value = "Meaningful" },
                                    new Choice() { Value = "Casual" },
                                    new Choice() { Value = "Meaningless" },
                                },
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateEmotionalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateEmotionalStepAsync");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            answers.Rating_Meaningfulness = stepContext.Context.Activity.Text;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the emotional factor (or enter your own word):"),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Dopamine Hit" },
                                    new Choice() { Value = "Inspirational" },
                                    new Choice() { Value = "Plain" },
                                    new Choice() { Value = "Anxious" },
                                    new Choice() { Value = "Despair" },
                                    new Choice() { Value = "Dread" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateImportanceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateImportanceStepAsync");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            answers.Rating_Emotional = stepContext.Context.Activity.Text;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the importance (or enter your own word):"),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Life-changing" },
                                    new Choice() { Value = "Influential" },
                                    new Choice() { Value = "Ordinary" },
                                    new Choice() { Value = "Waste of time" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateStrangenessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateStrangenessStepAsync");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            answers.Rating_Importance = stepContext.Context.Activity.Text;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the strangeness (or enter your own word):"),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Woo-woo weird" },
                                    new Choice() { Value = "Strange" },
                                    new Choice() { Value = "Normal" },
                                    new Choice() { Value = "Nothing" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateSynchronictyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateSynchronictyStepAsync");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            answers.Rating_Strangeness = stepContext.Context.Activity.Text;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the synchroncity factor (or enter your own word):"),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Dirk Gently" },
                                    new Choice() { Value = "Mind-blowing" },
                                    new Choice() { Value = "Somewhat" },
                                    new Choice() { Value = "Nothing" },
                                    new Choice() { Value = "Boredom" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> WriteReportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.WriteReportStepAsync");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            answers.Rating_Synchronicty = stepContext.Context.Activity.Text;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Lastly, write up your report. (Limited to being sent in one message.)") };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinishStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.FinishStepAsync");

            var callbackOptions = (CallbackOptions)stepContext.Options;
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context);
            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];
            answers.Report = ""+stepContext.Result;

            await StoreReportInDB(stepContext.Context, callbackOptions, answers);

            var intentSuggestions = "";
            if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
            {
                intentSuggestions = string.Join(", ", userProfile.IntentSuggestions) + "\n\n";
            }

            await Helpers.PostTripReportToRedditAsync("User Trip Report",
                callbackOptions.Messages[answers.PointNumberVisited].Replace(Environment.NewLine, "\n\n") +
                "Intent: " + answers.Intent + "\n\n" +
                intentSuggestions +
                "What 3 words address: " + callbackOptions.What3Words[answers.PointNumberVisited] + "\n\n" +
                "Astounding? " + answers.WasFuckingAmazing + "\n\n" +
                "Rating scale 1: " + answers.Rating_Meaningfulness + "\n\n" +
                "Rating scale 2: " + answers.Rating_Emotional + "\n\n" +
                "Rating scale 3: " + answers.Rating_Importance + "\n\n" +
                "Rating scale 4: " + answers.Rating_Strangeness + "\n\n" +
                "Rating scale 5: " + answers.Rating_Synchronicty
                );

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks for the report!"), cancellationToken);

            //await ((AdapterWithErrorHandler)stepContext.Context.Adapter).RepromptMainDialog(stepContext.Context, _mainDialog, cancellationToken, callbackOptions);
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
        }

        private async Task StoreReportInDB(ITurnContext context, CallbackOptions options, ReportAnswers answers)
        {
            var userProfile = await _userProfileAccessor.GetAsync(context);

            await Task.Run(() =>
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = Consts.DB_SERVER;
                builder.UserID = Consts.DB_USER;
                builder.Password = Consts.DB_PASSWORD;
                builder.InitialCatalog = Consts.DB_NAME;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    for (int i = 0; i < options.GeneratedPoints.Length; i++)
                    {
                        // TODO: would be good to be able to mark which point they visited if the idacount is > 1 (e.g. pairs, points generated with [>1] etc.)

                        var attractor = options.GeneratedPoints[i].X;

                        StringBuilder isb = new StringBuilder();
                        isb.Append("INSERT INTO reports (");
                        // isb.Append("id,"); Automatically incremented from the CREATE TABLE... id uniqueidentifier default NEWSEQUENTIALID() primary key command
                        isb.Append("user_id,");
                        isb.Append("platform,");
                        isb.Append("datetime,");
                        isb.Append("visited,");
                        isb.Append("point_type,");
                        if (!string.IsNullOrEmpty(answers.Intent))
                        {
                            isb.Append("intent_set,");
                        }
                        isb.Append("artifact_collected,");
                        isb.Append("fucking_amazing,");
                        isb.Append("rating_meaningfulness,");
                        isb.Append("rating_emotional,");
                        isb.Append("rating_importance,");
                        isb.Append("rating_strangeness,");
                        isb.Append("rating_synchroncity,");
                        isb.Append("text,");
                        isb.Append("photos,");
                        if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
                        {
                            isb.Append("intent_suggestions,");
                            isb.Append("time_intent_suggestions_set,");
                        }
                        isb.Append("what_3_words,");
                        isb.Append("short_hash_id,");
                        isb.Append("num_water_points_skipped,");
                        isb.Append("gid,");
                        isb.Append("tid,");
                        isb.Append("lid,");
                        isb.Append("idastep,");
                        isb.Append("idacount,");
                        isb.Append("type,");
                        isb.Append("x,");
                        isb.Append("y,");
                        isb.Append("center,");
                        isb.Append("latitude,");
                        isb.Append("longitude,");
                        isb.Append("distance,");
                        isb.Append("initial_bearing,");
                        isb.Append("final_bearing,");
                        isb.Append("side,");
                        isb.Append("distance_err,");
                        isb.Append("radiusM,");
                        isb.Append("number_points,");
                        isb.Append("mean,");
                        isb.Append("rarity,");
                        isb.Append("power_old,");
                        isb.Append("power,");
                        isb.Append("z_score,");
                        isb.Append("probability_single,");
                        isb.Append("integral_score,");
                        isb.Append("significance,");
                        isb.Append("probability");
                        isb.Append(") VALUES (");
                        isb.Append($"'{userProfile.UserId}',"); // sha256 hash of channel-issued userId
                        isb.Append($"'{(int)Enum.Parse(typeof(Enums.ChannelPlatform), context.Activity.ChannelId)}',");
                        isb.Append($"'{context.Activity.Timestamp}',"); // datetime
                        isb.Append($"'{(answers.WasPointVisited ? 1 : 0)}',"); // point visited or not?
                        isb.Append($"'{options.PointTypes[i].ToString()}',"); // point type enum as a string
                        if (!string.IsNullOrEmpty(answers.Intent))
                        {
                            isb.Append($"'{answers.Intent}',"); // intent set by user
                        }
                        isb.Append($"'{(answers.ArtifactCollected ? 1 : 0)}',"); // were artifact(s) collected?
                        isb.Append($"'{(answers.WasFuckingAmazing ? 1 : 0)}',"); // "yes" or "no" to the was it wow and astounding question
                        isb.Append($"'{answers.Rating_Meaningfulness}',"); // Rating_Meaningfulness
                        isb.Append($"'{answers.Rating_Emotional}',"); // Rating_Emotional
                        isb.Append($"'{answers.Rating_Importance}',"); // Rating_Importance
                        isb.Append($"'{answers.Rating_Strangeness}',"); // Rating_Strangeness
                        isb.Append($"'{answers.Rating_Synchronicty}',"); // Rating_Synchronicty
                        isb.Append($"'{answers.Report}',"); // text
                        isb.Append($"'{(answers.PhotoURLs != null ? string.Join(",", answers.PhotoURLs) : "")}',"); // photos
                        if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
                        {
                            isb.Append($"'{string.Join(",", userProfile.IntentSuggestions)}',"); // intent suggestions
                            isb.Append($"'{userProfile.TimeIntentSuggestionsSet}',");
                        }
                        isb.Append($"'{(!string.IsNullOrEmpty(options.What3Words[i])  ? options.What3Words[i] : "")}',");
                        isb.Append($"'{options.ShortCodes[i]}',");
                        isb.Append($"'{options.NumWaterPointsSkipped[i]}',");
                        isb.Append($"'{attractor.GID}',");
                        isb.Append($"'{attractor.TID}',");
                        isb.Append($"'{attractor.LID}',");
                        isb.Append($"'{i+1}',"); // idastep (which element in idacount array)
                        isb.Append($"'{options.GeneratedPoints.Length}',"); // total idacount
                        isb.Append($"'{attractor.type}',");
                        isb.Append($"'{attractor.x}',");
                        isb.Append($"'{attractor.y}',");
                        isb.Append($"geography::Point({attractor.center.point.latitude},{attractor.center.point.longitude}, 4326),");
                        isb.Append($"'{attractor.center.point.latitude}',");
                        isb.Append($"'{attractor.center.point.longitude}',");
                        isb.Append($"'{attractor.center.bearing.distance}',");
                        isb.Append($"'{attractor.center.bearing.initialBearing}',");
                        isb.Append($"'{attractor.center.bearing.finalBearing}',");
                        isb.Append($"'{attractor.side}',");
                        isb.Append($"'{attractor.distanceErr}',");
                        isb.Append($"'{attractor.radiusM}',");
                        isb.Append($"'{attractor.n}',");
                        isb.Append($"'{attractor.mean}',");
                        isb.Append($"'{attractor.rarity}',");
                        isb.Append($"'{attractor.power_old}',");
                        isb.Append($"'{attractor.power}',");
                        isb.Append($"'{attractor.z_score}',");
                        isb.Append($"'{attractor.probability_single}',");
                        isb.Append($"'{attractor.integral_score}',");
                        isb.Append($"'{attractor.significance}',");
                        isb.Append($"'{attractor.probability}'");
                        isb.Append(")");
                        var insertSql = isb.ToString();
                        Console.WriteLine("SQL:" + insertSql.ToString());

                        using (SqlCommand command = new SqlCommand(insertSql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                string commandResult = "";
                                while (reader.Read())
                                {
                                    commandResult += $"{reader.GetString(0)} {reader.GetString(1)}\n";
                                }
                                context.SendActivityAsync(commandResult);
                            }
                        }
                    }
                }
            });
        }
    }
}
