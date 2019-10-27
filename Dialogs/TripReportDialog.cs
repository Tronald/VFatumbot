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

        private const string ReportAnswersKey = "value-ReportAnswers";

        public class ReportAnswers {
            public bool IsPointVisited { get; set; }
            public TripRating Rating { get; set; }
            public string Intent { get; set; }
            public string Report { get; set; }
            public string[] PhotoURLs { get; set; }
        }

        public TripReportDialog(IStatePropertyAccessor<UserProfile> userProfileAccessor, ILogger<MainDialog> logger) : base(nameof(TripReportDialog))
        {
            _logger = logger;
            _userProfileAccessor = userProfileAccessor;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ReportYesOrNoStepAsync,
                RateTripStepAsync,
                AskIntentStepAsync,
                InputIntentStepAsync,
                WriteReportSendPhotosStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ReportYesOrNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("TripReportDialog.ReportYesOrNoStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Did you visit this point and would you like to share your experience?"),
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
                                    Value = "Yes and share!",
                                    Synonyms = new List<string>()
                                                    {
                                                    }
                                },
                                new Choice() {
                                    Value = "Yes sans sharing",
                                    Synonyms = new List<string>()
                                                    {
                                                    }
                                }
                }
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RateTripStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TripReportDialog.RateTripStepAsync[{((FoundChoice)stepContext.Result).Value}]");

            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Yes and share":
                    // Go and start asking them about their trip
                    await stepContext.Context.SendActivityAsync("How would you rate your trip?");
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                case "Yes sans sharing":
                    await stepContext.Context.SendActivityAsync("Hope you had a fun trip!");

                    // At least mark the point as a visited one
                    await StoreReportInDB(stepContext.Context, (CallbackOptions)stepContext.Options, new ReportAnswers() { IsPointVisited = true });

                    await stepContext.EndDialogAsync();
                    return await stepContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);
                case "No":
                default:
                    await StoreReportInDB(stepContext.Context, (CallbackOptions) stepContext.Options, new ReportAnswers());
                    await stepContext.EndDialogAsync();
                    return await stepContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> AskIntentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("TripReportDialog.AskIntentStepAsync");
            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> InputIntentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("TripReportDialog.InputIntentStepAsync");
            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> WriteReportSendPhotosStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("TripReportDialog.WriteReportSendPhotosStepAsync");
            return await stepContext.EndDialogAsync();
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
                        isb.Append("rating,");
                        isb.Append("text,");
                        isb.Append("photos,");
                        if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
                        {
                            isb.Append("intent_suggestions,");
                        }
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
                        isb.Append($"'{(answers.IsPointVisited ? 1 : 0)}',"); // point visited or not?
                        isb.Append($"'{(int)answers.Rating}',"); // trip rating
                        isb.Append($"'{answers.Report}',"); // text
                        isb.Append($"'{(answers.PhotoURLs != null ? string.Join(",", answers.PhotoURLs) : "")}',"); // photos
                        if (userProfile.IntentSuggestions != null && userProfile.IntentSuggestions.Length > 0)
                        {
                            // TODO: ideally we'd like to make sure that the intent suggestions saved in UserProfile
                            // actually were made before the points being reported upon here.
                            // Currently logic just takes the last intent suggestions saved to the UserProfile
                            isb.Append($"'{string.Join(",", userProfile.IntentSuggestions)}',"); // intent suggestions
                        }
                        isb.Append($"'{options.NumWaterPointsSkipped}',");
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
