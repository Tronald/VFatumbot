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

            public bool SkipGetIntentStep { get; set; }
            public string Intent { get; set; }

            public bool ArtifactCollected { get; set; }

            public bool WasFuckingAmazing { get; set; }

            public TripRating RatingScale1 { get; set; }
            public TripRating RatingScale2 { get; set; }
            public TripRating RatingScale3 { get; set; }
            public TripRating RatingScale4 { get; set; }
            public TripRating RatingScale5 { get; set; }

            public string Report { get; set; }
            public string[] PhotoURLs { get; set; }
        }

        public TripReportDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, MainDialog mainDialog, ILogger<MainDialog> logger) : base(nameof(TripReportDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;
            _mainDialog = mainDialog;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ReportYesOrNoStepAsync,
                StartReportStepAsync,
                SetIntentYesOrNoStepAsync,
                GetIntentStepAsync,
                ArtifactsCollectedYesOrNoStepAsync,
                FuckingAmazingYesOrNoStepAsync,
                RateScale1StepAsync,
                RateScale2StepAsync,
                RateScale3StepAsync,
                RateScale4StepAsync,
                RateScale5StepAsync,
                WriteReportStepAsync,
                FinishStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
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

                    stepContext.Values[ReportAnswersKey] = new ReportAnswers() { WasPointVisited = true };

                    switch (callbackOptions.PointType)
                    {
                        case PointTypes.Attractor:
                        case PointTypes.Void:
                        case PointTypes.Anomaly:
                        case PointTypes.Pair:
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
            _logger.LogInformation($"TripReportDialog.SetIntentYesOrNoStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            if (answers.SkipGetIntentStep)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }

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
                Prompt = MessageFactory.Text("Did you take back any artifacts?"),
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

        private async Task<DialogTurnResult> RateScale1StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateScale1StepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes":
                    answers.WasFuckingAmazing = true;
                    break;
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the meaningfulness of your trip:"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Enriching" },
                                    new Choice() { Value = "Strange" },
                                    new Choice() { Value = "Casual" },
                                    new Choice() { Value = "Meaningless" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateScale2StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateScale2StepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Enriching":
                    answers.RatingScale1 = TripRating.A;
                    break;
                case "Strange":
                    answers.RatingScale1 = TripRating.B;
                    break;
                case "Casual":
                    answers.RatingScale1 = TripRating.C;
                    break;
                case "Meaningless":
                    answers.RatingScale1 = TripRating.D;
                    break;
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the emotional factor:"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Inspirational" },
                                    new Choice() { Value = "Plain" },
                                    new Choice() { Value = "Despair" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateScale3StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateScale3StepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Inspirational":
                    answers.RatingScale2 = TripRating.A;
                    break;
                case "Plain":
                    answers.RatingScale2 = TripRating.B;
                    break;
                case "Despair":
                    answers.RatingScale2 = TripRating.C;
                    break;
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the importance:"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
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

        private async Task<DialogTurnResult> RateScale4StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateScale4StepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Life-changing":
                    answers.RatingScale3 = TripRating.A;
                    break;
                case "Influential":
                    answers.RatingScale3 = TripRating.B;
                    break;
                case "Ordinary":
                    answers.RatingScale3 = TripRating.C;
                    break;
                case "Waste of time":
                    answers.RatingScale3 = TripRating.D;
                    break;

            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the strangeness:"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Woo-woo weird" },
                                    new Choice() { Value = "Pretty normal" },
                                    new Choice() { Value = "Nothing" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateScale5StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateScale5StepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Woo-woo weird":
                    answers.RatingScale4 = TripRating.A;
                    break;
                case "Pretty normal":
                    answers.RatingScale4 = TripRating.B;
                    break;
                case "Nothing":
                    answers.RatingScale4 = TripRating.C;
                    break;
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Rate the synchroncity factor:"),
                RetryPrompt = MessageFactory.Text("That is not a valid choice."),
                Choices = new List<Choice>()
                                {
                                    new Choice() { Value = "Dirk Gently" },
                                    new Choice() { Value = "Mind-blowing" },
                                    new Choice() { Value = "Somewhat" },
                                    new Choice() { Value = "Boredom" },
                                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> WriteReportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.WriteReportStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            var answers = (ReportAnswers)stepContext.Values[ReportAnswersKey];

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Dirk Gently":
                    answers.RatingScale5 = TripRating.A;
                    break;
                case "Mind-blowing":
                    answers.RatingScale5 = TripRating.B;
                    break;
                case "Somewhat":
                    answers.RatingScale5 = TripRating.C;
                    break;
                case "Boredom":
                    answers.RatingScale5 = TripRating.D;
                    break;

            }

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Lastly, write up your report.") };
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
                callbackOptions.Messages[0].Replace(Environment.NewLine, "\n\n") +
                "Intent: " + answers.Intent + "\n\n" +
                intentSuggestions +
                "Astounding? " + answers.WasFuckingAmazing + "\n\n" +
                "Rating scale 1: " + answers.RatingScale1 + "\n\n" +
                "Rating scale 2: " + answers.RatingScale2 + "\n\n" +
                "Rating scale 3: " + answers.RatingScale3 + "\n\n" +
                "Rating scale 4: " + answers.RatingScale4 + "\n\n" +
                "Rating scale 5: " + answers.RatingScale5
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
                        if (!string.IsNullOrEmpty(answers.Intent))
                        {
                            isb.Append("intent_set,");
                        }
                        isb.Append("artifact_collected,");
                        isb.Append("fucking_amazing,");
                        isb.Append("rating_scale_1,");
                        isb.Append("rating_scale_2,");
                        isb.Append("rating_scale_3,");
                        isb.Append("rating_scale_4,");
                        isb.Append("rating_scale_5,");
                        isb.Append("text,");
                        isb.Append("photos,");
                        if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
                        {
                            isb.Append("intent_suggestions,");
                            isb.Append("time_intent_suggestions_set,");
                        }
                        isb.Append("what_3_words,");
                        isb.Append("num_water_points_skipped,");
                        isb.Append("gid,");
                        isb.Append("tid,");
                        isb.Append("lid,");
                        isb.Append("idastep,");
                        isb.Append("idacount,");
                        isb.Append("type,");
                        isb.Append("x,");
                        isb.Append("y,");
                        //isb.Append("center,");
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
                        if (!string.IsNullOrEmpty(answers.Intent))
                        {
                            isb.Append($"'{answers.Intent}',"); // intent set by user
                        }
                        isb.Append($"'{(answers.ArtifactCollected ? 1 : 0)}',"); // were artifact(s) collected?
                        isb.Append($"'{(answers.WasFuckingAmazing ? 1 : 0)}',"); // "yes" or "no" to the was it wow and astounding question
                        isb.Append($"'{(int)answers.RatingScale1}',"); // rating scale 1
                        isb.Append($"'{(int)answers.RatingScale2}',"); // rating scale 2
                        isb.Append($"'{(int)answers.RatingScale3}',"); // rating scale 3
                        isb.Append($"'{(int)answers.RatingScale4}',"); // rating scale 4
                        isb.Append($"'{(int)answers.RatingScale5}',"); // rating scale 5
                        isb.Append($"'{answers.Report}',"); // text
                        isb.Append($"'{(answers.PhotoURLs != null ? string.Join(",", answers.PhotoURLs) : "")}',"); // photos
                        if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
                        {
                            isb.Append($"'{string.Join(",", userProfile.IntentSuggestions)}',"); // intent suggestions
                            isb.Append($"'{userProfile.TimeIntentSuggestionsSet}',");
                        }
                        isb.Append($"'{(!string.IsNullOrEmpty(options.What3Words[i])  ? options.What3Words[i] : "")}',");
                        isb.Append($"'{options.NumWaterPointsSkipped[i]}',");
                        isb.Append($"'{attractor.GID}',");
                        isb.Append($"'{attractor.TID}',");
                        isb.Append($"'{attractor.LID}',");
                        isb.Append($"'{i+1}',"); // idastep (which element in idacount array)
                        isb.Append($"'{options.GeneratedPoints.Length}',"); // total idacount
                        isb.Append($"'{attractor.type}',");
                        isb.Append($"'{attractor.x}',");
                        isb.Append($"'{attractor.y}',");
                        //isb.Append($"'{attractor.center}',");
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
