using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using static VFatumbot.AdapterWithErrorHandler;
using static VFatumbot.BotLogic.Enums;
using static VFatumbot.BotLogic.FatumFunctions;

namespace VFatumbot.BotLogic
{
    public class ActionHandler
    {
        public void DispatchWorkerThread(DoWorkEventHandler handler)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += handler;
            backgroundWorker.RunWorkerAsync();
        }

        public async Task ParseSlashCommands(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            var command = turnContext.Activity.Text.ToLower();
            if (command.StartsWith("/ongshat", StringComparison.InvariantCulture))
            {
                await turnContext.SendActivityAsync("You're not authorized to do that!");
                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken);
            }
            else if (command.StartsWith("/getattractor", StringComparison.InvariantCulture))
            {
                await AttractorActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getvoid", StringComparison.InvariantCulture))
            {
                await VoidActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getpair", StringComparison.InvariantCulture))
            {
                await PairActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getattractor", StringComparison.InvariantCulture))
            {
                await AttractorActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getpseudo", StringComparison.InvariantCulture))
            {
                await PseudoActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getquantum", StringComparison.InvariantCulture))
            {
                await QuantumActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getqtime", StringComparison.InvariantCulture))
            {
                await QuantumActionAsync(turnContext, userProfile, cancellationToken, mainDialog, true);
            }
            else if (command.StartsWith("/getida", StringComparison.InvariantCulture) ||
                     command.StartsWith("/getanomaly", StringComparison.InvariantCulture))
            {
                await AnomalyActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/scanida", StringComparison.InvariantCulture))
            {
                await AnomalyActionAsync(turnContext, userProfile, cancellationToken, mainDialog, true);
            }
            else if (command.StartsWith("/scanattractor", StringComparison.InvariantCulture))
            {
                await AttractorActionAsync(turnContext, userProfile, cancellationToken, mainDialog, true);
            }
            else if (command.StartsWith("/scanvoid", StringComparison.InvariantCulture))
            {
                await VoidActionAsync(turnContext, userProfile, cancellationToken, mainDialog, true);
            }
            else if (command.StartsWith("/scanpair", StringComparison.InvariantCulture))
            {
                await PairActionAsync(turnContext, userProfile, cancellationToken, mainDialog, true);
            }
            else if (command.StartsWith("/getpoint", StringComparison.InvariantCulture) ||
                     command.StartsWith("/blindspot", StringComparison.InvariantCulture) ||
                     command.StartsWith("/blind", StringComparison.InvariantCulture))
            {
                await MysteryPointActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/setradius", StringComparison.InvariantCulture))
            {
                if (command.Contains(" "))
                {
                    string newRadiusStr = command.Replace(command.Substring(0, command.IndexOf(" ", StringComparison.InvariantCulture)), "");
                    int oldRadius = userProfile.Radius, inputtedRadius;
                    if (Int32.TryParse(newRadiusStr, out inputtedRadius))
                    {
                        if (inputtedRadius < Consts.RADIUS_MIN)
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Radius must be more than or equal to {Consts.RADIUS_MIN}m"), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken);
                            return;
                        }
                        if (inputtedRadius > Consts.RADIUS_MAX)
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Radius must be less than or equal to {Consts.RADIUS_MAX}m"), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken);
                            return;
                        }

                        userProfile.Radius = inputtedRadius;
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Changed radius from {oldRadius}m to {userProfile.Radius}m"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Invalid radius"), cancellationToken);
                    }
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Your current radius from is {userProfile.Radius}m"), cancellationToken);
                }

                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken);
            }
            else if (command.Equals("/kpi"))
            {
                // Quick hack to see KPIs

                // TODO: get more like DAU etc

                // How many reports today?
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

                        StringBuilder ssb = new StringBuilder();
                        ssb.Append("SELECT COUNT(*) FROM ");
#if RELEASE_PROD
                        ssb.Append("reports");
#else
                        ssb.Append("reports_dev");
#endif
                        ssb.Append(";");
                        Console.WriteLine("SQL:" + ssb.ToString());

                        using (SqlCommand command = new SqlCommand(ssb.ToString(), connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                string commandResult = "";
                                while (reader.Read())
                                {
                                    commandResult += $"{reader.GetInt32(0)}\n";
                                }
                                turnContext.SendActivityAsync(MessageFactory.Text(commandResult));
                            }
                        }
                    }
                });

                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken);
        }
    }

        public async Task AttractorActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
        {
            int idacou = 1;
            if (turnContext.Activity.Text.Contains("["))
            {
                string[] buf = turnContext.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                userProfile.IsScanning = true;
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        int numWaterPointsSkipped = 0;

                    redo:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "attractor", idacou);
                        if (ida.Length > 0)
                        {
                            var shortCodesArray = new string[ida.Count()];
                            var messagesArray = new string[ida.Count()];
                            var pointTypesArray = new PointTypes[ida.Count()];
                            var numWaterPointsSkippedArray = new int[ida.Count()];
                            var what3WordsArray = new string[ida.Count()];
                            var nearestPlacesArray = new string[ida.Count()];

                            for (int i = 0; i < ida.Count(); i++)
                            {
                                var incoords = new double[] { ida[i].X.center.point.latitude, ida[i].X.center.point.longitude };

                                // If water points are set to be skipped, and there's only 1 point in the result array, try again else just exclude those from the results
                                if (!userProfile.IsIncludeWaterPoints)
                                {
                                    var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(incoords);
                                    if (isWaterPoint)
                                    {
                                        numWaterPointsSkipped++;

                                        if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                        {
                                            await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                                            return;
                                        }

                                        if (ida.Length == 1)
                                        {
                                            await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                            goto redo;
                                        }

                                        continue;
                                    }
                                }

                                shortCodesArray[i] = Helpers.Crc32Hash(context.Activity.From.Id + context.Activity.Timestamp);
                                mesg = (idacou > 1 ? ("#"+(i+1)+" ") : "") + Tolog(turnContext, "attractor", ida[i], shortCodesArray[i]);
                                await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                if (numWaterPointsSkipped > 0)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped: " + numWaterPointsSkipped), cancellationToken);
                                }

                                messagesArray[i] = mesg;
                                pointTypesArray[i] = PointTypes.Attractor;
                                numWaterPointsSkippedArray[i] = numWaterPointsSkipped;
                                what3WordsArray[i] = ""+w3wResult.words;
                                nearestPlacesArray[i] = "" + w3wResult.nearestPlace;

                                await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
                                await Helpers.SendPushNotification(userProfile, "Point Generated", mesg);
                            }

                            CallbackOptions callbackOptions = new CallbackOptions()
                            {
                                StartTripReportDialog = true,
                                ShortCodes = shortCodesArray,
                                Messages = messagesArray,
                                PointTypes = pointTypesArray,
                                GeneratedPoints = ida,
                                NumWaterPointsSkipped = numWaterPointsSkippedArray,
                                What3Words = what3WordsArray,
                                NearestPlaces = nearestPlacesArray
                            };
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, callbackOptions);
                        }
                        else
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                        }
                    }, cancellationToken);
            });
        }

        public async Task VoidActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
        {
            int idacou = 1;
            if (turnContext.Activity.Text.Contains("["))
            {
                string[] buf = turnContext.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                userProfile.IsScanning = true;
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        int numWaterPointsSkipped = 0;

                    redo:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "void", idacou);
                        if (ida.Length > 0)
                        {
                            var shortCodesArray = new string[ida.Count()];
                            var messagesArray = new string[ida.Count()];
                            var pointTypesArray = new PointTypes[ida.Count()];
                            var numWaterPointsSkippedArray = new int[ida.Count()];
                            var what3WordsArray = new string[ida.Count()];
                            var nearestPlacesArray = new string[ida.Count()];

                            for (int i = 0; i < ida.Count(); i++)
                            {
                                var incoords = new double[] { ida[i].X.center.point.latitude, ida[i].X.center.point.longitude };

                                // If water points are set to be skipped, and there's only 1 point in the result array, try again else just exclude those from the results
                                if (!userProfile.IsIncludeWaterPoints)
                                {
                                    var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(incoords);
                                    if (isWaterPoint)
                                    {
                                        numWaterPointsSkipped++;

                                        if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                        {
                                            await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                                            return;
                                        }

                                        if (ida.Length == 1)
                                        {
                                            await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                            goto redo;
                                        }

                                        continue;
                                    }
                                }

                                shortCodesArray[i] = Helpers.Crc32Hash(context.Activity.From.Id + context.Activity.Timestamp);
                                mesg = (idacou > 1 ? ("#" + (i + 1) + " ") : "") + Tolog(turnContext, "void", ida[i], shortCodesArray[i]);
                                await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                if (numWaterPointsSkipped > 0)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped: " + numWaterPointsSkipped), cancellationToken);
                                }

                                messagesArray[i] = mesg;
                                pointTypesArray[i] = PointTypes.Void;
                                numWaterPointsSkippedArray[i] = numWaterPointsSkipped;
                                what3WordsArray[i] = "" + w3wResult.words;
                                nearestPlacesArray[i] = "" + w3wResult.nearestPlace;

                                await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
                                await Helpers.SendPushNotification(userProfile, "Point Generated", mesg);
                            }

                            CallbackOptions callbackOptions = new CallbackOptions()
                            {
                                StartTripReportDialog = true,
                                ShortCodes = shortCodesArray,
                                Messages = messagesArray,
                                PointTypes = pointTypesArray,
                                GeneratedPoints = ida,
                                NumWaterPointsSkipped = numWaterPointsSkippedArray,
                                What3Words = what3WordsArray,
                                NearestPlaces = nearestPlacesArray
                            };
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, callbackOptions);
                        }
                        else
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                        }
                    }, cancellationToken);
            });
        }

        public async Task LocationActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Your current radius is {userProfile.Radius}m"), cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text($"Your current location is {userProfile.Latitude},{userProfile.Longitude}"), cancellationToken);

            var incoords = new double[] { userProfile.Latitude, userProfile.Longitude };

            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

            await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
        }

        public async Task AnomalyActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = true)
        {
            int idacou = 1;
            if (turnContext.Activity.Text.Contains("["))
            {
                string[] buf = turnContext.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                userProfile.IsScanning = true;
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        int numWaterPointsSkipped = 0;

                    redo:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "any", idacou);
                        if (ida.Length > 0)
                        {
                            var shortCodesArray = new string[ida.Count()];
                            var messagesArray = new string[ida.Count()];
                            var pointTypesArray = new PointTypes[ida.Count()];
                            var numWaterPointsSkippedArray = new int[ida.Count()];
                            var what3WordsArray = new string[ida.Count()];
                            var nearestPlacesArray = new string[ida.Count()];

                            for (int i = 0; i < ida.Count(); i++)
                            {
                                var incoords = new double[] { ida[i].X.center.point.latitude, ida[i].X.center.point.longitude };

                                // If water points are set to be skipped, and there's only 1 point in the result array, try again else just exclude those from the results
                                if (!userProfile.IsIncludeWaterPoints)
                                {
                                    var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(incoords);
                                    if (isWaterPoint)
                                    {
                                        numWaterPointsSkipped++;

                                        if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                        {
                                            await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                                            return;
                                        }

                                        if (ida.Length == 1)
                                        {
                                            await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                            goto redo;
                                        }

                                        continue;
                                    }
                                }

                                shortCodesArray[i] = Helpers.Crc32Hash(context.Activity.From.Id + context.Activity.Timestamp);
                                mesg = (idacou > 1 ? ("#" + (i + 1) + " ") : "") + Tolog(turnContext, "ida", ida[i], shortCodesArray[i]);
                                await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                if (numWaterPointsSkipped > 0)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped: " + numWaterPointsSkipped), cancellationToken);
                                }

                                messagesArray[i] = mesg;
                                pointTypesArray[i] = PointTypes.Anomaly;
                                numWaterPointsSkippedArray[i] = numWaterPointsSkipped;
                                what3WordsArray[i] = "" + w3wResult.words;
                                nearestPlacesArray[i] = "" + w3wResult.nearestPlace;

                                await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
                                await Helpers.SendPushNotification(userProfile, "Point Generated", mesg);
                            }

                            CallbackOptions callbackOptions = new CallbackOptions()
                            {
                                StartTripReportDialog = true,
                                ShortCodes = shortCodesArray,
                                Messages = messagesArray,
                                PointTypes = pointTypesArray,
                                GeneratedPoints = ida,
                                NumWaterPointsSkipped = numWaterPointsSkippedArray,
                                What3Words = what3WordsArray,
                                NearestPlaces = nearestPlacesArray
                            };
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, callbackOptions);
                        }
                        else
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                        }
                    }, cancellationToken);
            });
        }

        public async Task IntentSuggestionActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. Quantumly randomizing the English dictionary for you."), cancellationToken);

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        string[] intentSuggestions = await Helpers.GetIntentSuggestionsAsync();
                        var suggestionsStr = string.Join(", ", intentSuggestions);

                        await turnContext.SendActivityAsync(MessageFactory.Text("Intent suggestions: " + suggestionsStr), cancellationToken);
                        await Helpers.SendPushNotification(userProfile, "Intent Suggestions", suggestionsStr);

                        CallbackOptions callbackOptions = new CallbackOptions()
                        {
                            UpdateIntentSuggestions = true,
                            IntentSuggestions = intentSuggestions,
                            TimeIntentSuggestionsSet = turnContext.Activity.Timestamp.ToString()
                        };
                        await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, callbackOptions);
                    }, cancellationToken);
            });
        }

        public async Task QuantumActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool suggestTime = false)
        {
            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        int numWaterPointsSkipped = 0;

                    redo:
                        double[] incoords = GetQuantumRandom(userProfile.Latitude, userProfile.Longitude, userProfile.Radius);

                        // Skip water points?
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(incoords);
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redo;
                            }
                        }

                        string shortCode = Helpers.Crc32Hash(turnContext.Activity.From.Id + turnContext.Activity.Timestamp);
                        string mesg = Tolog(turnContext, "random", (float)incoords[0], (float)incoords[1], suggestTime ? "qtime" : "quantum", shortCode);
                        await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                        dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                        await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
                        await Helpers.SendPushNotification(userProfile, "Point Generated", mesg);

                        CallbackOptions callbackOptions = new CallbackOptions()
                        {
                            StartTripReportDialog = true,
                            ShortCodes = new string[] { shortCode },
                            Messages = new string[] { mesg },
                            PointTypes = new PointTypes[] { PointTypes.Quantum },
                            GeneratedPoints = new FinalAttractor[] {
                                // FinalAttractor is just a good wrapper to carry point/type info across the boundary to the TripReportDialog
                                new FinalAttractor()
                                {
                                    X = new FinalAttr()
                                    {
                                        center = new Coordinate()
                                        {
                                            point = new LatLng(incoords[0], incoords[1]),
                                        },
                                    }
                                }
                            },
                            NumWaterPointsSkipped = new int[] { numWaterPointsSkipped },
                            What3Words = new string[] { w3wResult.words },
                            NearestPlaces = new string[] { w3wResult.nearestPlace }
                        };
                        await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken, callbackOptions);
                    }, cancellationToken);
            });
        }

        public async Task PseudoActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        int numWaterPointsSkipped = 0;

                    redo:
                        double[] incoords = GetPseudoRandom(userProfile.Latitude, userProfile.Longitude, userProfile.Radius);

                        // Skip water points?
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(incoords);
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redo;
                            }
                        }

                        string shortCode = Helpers.Crc32Hash(turnContext.Activity.From.Id + turnContext.Activity.Timestamp);
                        string mesg = Tolog(turnContext, "random", (float)incoords[0], (float)incoords[1], "pseudo", shortCode);
                        await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                        dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                        await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
                        await Helpers.SendPushNotification(userProfile, "Point Generated", mesg);

                        CallbackOptions callbackOptions = new CallbackOptions()
                        {
                            StartTripReportDialog = true,
                            ShortCodes = new string[] { shortCode },
                            Messages = new string[] { mesg },
                            PointTypes = new PointTypes[] { PointTypes.QuantumTime },
                            GeneratedPoints = new FinalAttractor[] {
                                // FinalAttractor is just a good wrapper to carry point/type info across the boundary to the TripReportDialog
                                new FinalAttractor()
                                {
                                    X = new FinalAttr()
                                    {
                                        center = new Coordinate()
                                        {
                                            point = new LatLng(incoords[0], incoords[1]),
                                        },
                                    }
                                }
                            },
                            NumWaterPointsSkipped = new int[] { numWaterPointsSkipped },
                            What3Words = new string[] { w3wResult.words }
                        };
                        await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken, callbackOptions);
                    }, cancellationToken);
            });
        }

        public async Task PairActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
        {
            int idacou = 1;
            if (turnContext.Activity.Text.Contains("["))
            {
                string[] buf = turnContext.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                userProfile.IsScanning = true;
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        int numAttWaterPointsSkipped = 0;
                        int numVoiWaterPointsSkipped = 0;
                        string mesg = "";

                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        FinalAttractor[] att = SortIDA(ida, "attractor", idacou);
                        FinalAttractor[] voi = SortIDA(ida, "void", idacou);
                        if (att.Count() > voi.Count())
                        {
                            idacou = voi.Count();
                        }
                        else
                        {
                            idacou = att.Count();
                        }
                        if (idacou > 0)
                        {
                            var attShortCodesArray = new string[ida.Count()];
                            var attMessagesArray = new string[ida.Count()];
                            var attPointTypesArray = new PointTypes[ida.Count()];
                            var attNumWaterPointsSkippedArray = new int[ida.Count()];
                            var attWhat3WordsArray = new string[ida.Count()];
                            var attNearestPlacesArray = new string[ida.Count()];

                            var voiShortCodesArray = new string[ida.Count()];
                            var voiMessagesArray = new string[ida.Count()];
                            var voiPointTypesArray = new PointTypes[ida.Count()];
                            var voiNumWaterPointsSkippedArray = new int[ida.Count()];
                            var voiWhat3WordsArray = new string[ida.Count()];
                            var voiNearestPlacesArray = new string[ida.Count()];

                            for (int i = 0; i < idacou; i++)
                            {
                                // TODO: The "(idacou > 1 ? ("#"+(i+1)+" ") : "")" logic below for pairs needs to be split between attractor/void

                                var incoords = new double[] { att[i].X.center.point.latitude, att[i].X.center.point.longitude };
                                if (!userProfile.IsIncludeWaterPoints && await Helpers.IsWaterCoordinatesAsync(incoords))
                                {
                                    numAttWaterPointsSkipped++;
                                }
                                else
                                {
                                    attShortCodesArray[i] = Helpers.Crc32Hash(context.Activity.From.Id + context.Activity.Timestamp);

                                    mesg = (idacou > 1 ? ("#" + (i + 1) + " ") : "") + Tolog(turnContext, "attractor", att[i], attShortCodesArray[i]);
                                    await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                                    dynamic w3wResult1 = await Helpers.GetWhat3WordsAddressAsync(incoords);
                                    await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult1), cancellationToken);

                                    attMessagesArray[i] = mesg;
                                    attPointTypesArray[i] = PointTypes.PairAttractor;
                                    attNumWaterPointsSkippedArray[i] = numAttWaterPointsSkipped;
                                    attWhat3WordsArray[i] = "" + w3wResult1.words;
                                    attNearestPlacesArray[i] = "" + w3wResult1.nearestPlace;
                                }

                                incoords = new double[] { voi[i].X.center.point.latitude, voi[i].X.center.point.longitude };
                                if (!userProfile.IsIncludeWaterPoints && await Helpers.IsWaterCoordinatesAsync(incoords))
                                {
                                    numVoiWaterPointsSkipped++;
                                }
                                else
                                {
                                    voiShortCodesArray[i] = Helpers.Crc32Hash(context.Activity.From.Id + context.Activity.Timestamp);

                                    mesg = (idacou > 1 ? ("#" + (i + 1) + " ") : "") + Tolog(turnContext, "void", voi[i], voiShortCodesArray[i]);
                                    await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                                    dynamic w3wResult2 = await Helpers.GetWhat3WordsAddressAsync(incoords);
                                    await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult2), cancellationToken);

                                    voiMessagesArray[i] = mesg;
                                    voiPointTypesArray[i] = PointTypes.PairVoid;
                                    voiNumWaterPointsSkippedArray[i] = numAttWaterPointsSkipped;
                                    voiWhat3WordsArray[i] = "" + w3wResult2.words;
                                    voiNearestPlacesArray[i] = "" + w3wResult2.nearestPlace;
                                }
                            }

                            if (numAttWaterPointsSkipped > 1)
                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of attractor water points skipped: " + numAttWaterPointsSkipped), cancellationToken);
                            if (numVoiWaterPointsSkipped > 1)
                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of void water points skipped: " + numVoiWaterPointsSkipped), cancellationToken);

                            await Helpers.SendPushNotification(userProfile, "Pair of Points Generated", attMessagesArray[0]); // just send one notification

                            CallbackOptions callbackOptions = new CallbackOptions()
                            {
                                StartTripReportDialog = true,
                                ShortCodes = attShortCodesArray.Concat(voiShortCodesArray).ToArray(),
                                Messages = attMessagesArray.Concat(voiMessagesArray).ToArray(),
                                PointTypes = attPointTypesArray.Concat(voiPointTypesArray).ToArray(),
                                GeneratedPoints = att.Concat(voi).ToArray(),
                                NumWaterPointsSkipped = attNumWaterPointsSkippedArray.Concat(voiNumWaterPointsSkippedArray).ToArray(),
                                What3Words = attWhat3WordsArray.Concat(voiWhat3WordsArray).ToArray(),
                                NearestPlaces = attNearestPlacesArray.Concat(voiNearestPlacesArray).ToArray(),
                            };
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, callbackOptions);
                        }
                        else
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, new CallbackOptions() { ResetFlag = doScan });
                        }
                    }, cancellationToken);
            });
        }

        public async Task MysteryPointActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID, turnContext.Activity.GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        double[] incoords = new double[2];
                        int numWaterPointsSkipped = 0;

                    redoIDA:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, -1/*not used?*/);

                        FinalAttractor[] att = SortIDA(ida, "attractor", 1);
                        if (att.Length > 0 && !userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(new double[] { att[0].X.center.point.latitude, att[0].X.center.point.longitude });
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoIDA;
                            }
                        }

                        FinalAttractor[] voi = SortIDA(ida, "void", 1);
                        if (voi.Length > 0 && !userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(new double[] { voi[0].X.center.point.latitude, voi[0].X.center.point.longitude });
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoIDA;
                            }
                        }

                        ida = SortIDA(ida, "any", 1);

                    redoPseudo:
                        double[] pcoords = GetPseudoRandom(userProfile.Location.latitude, userProfile.Location.longitude, userProfile.Radius);
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(pcoords);
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoPseudo;
                            }
                        }

                    redoQuantum:
                        double[] qcoords = GetQuantumRandom(userProfile.Location.latitude, userProfile.Location.longitude, userProfile.Radius);
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(qcoords);
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but water points. Try again later."), cancellationToken);
                                    await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoQuantum;
                            }
                        }

                        Random rn = new Random();
                        int mtd = rn.Next(100);
                        var shortCode = Helpers.Crc32Hash(context.Activity.From.Id + context.Activity.Timestamp);

                        if (mtd < 20)
                        {
                            incoords[0] = pcoords[0];
                            incoords[1] = pcoords[1];
                            mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "pseudo", shortCode);
                        }
                        if ((mtd < 40) && (mtd > 20))
                        {
                            if (att.Count() > 0)
                            {
                                incoords[0] = att[0].X.center.point.latitude;
                                incoords[1] = att[0].X.center.point.longitude;
                                mesg = Tolog(turnContext, "blind", att[0], shortCode);
                            }
                            else
                            {
                                incoords[0] = pcoords[0];
                                incoords[1] = pcoords[1];
                                mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "pseudo", shortCode);
                            }
                        }
                        if ((mtd < 60) && (mtd > 40))
                        {
                            incoords[0] = qcoords[0];
                            incoords[1] = qcoords[1];
                            mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "quantum", shortCode);
                        }
                        if ((mtd < 80) && (mtd > 60))
                        {
                            if (voi.Count() > 0)
                            {
                                incoords[0] = voi[0].X.center.point.latitude;
                                incoords[1] = voi[0].X.center.point.longitude;
                                mesg = Tolog(turnContext, "blind", voi[0], shortCode);
                            }
                            else
                            {
                                mtd = 90;
                            }
                        }
                        if ((mtd < 100) && (mtd > 80))
                        {
                            if (ida.Count() > 0)
                            {
                                incoords[0] = ida[0].X.center.point.latitude;
                                incoords[1] = ida[0].X.center.point.longitude;
                                mesg = Tolog(turnContext, "blind", ida[0], shortCode);
                            }
                            else
                            {
                                incoords[0] = qcoords[0];
                                incoords[1] = qcoords[1];
                                mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "quantum", shortCode);
                            }
                        }

                        await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                        dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                        await turnContext.SendActivityAsync(CardFactory.CreateLocationCardsReply(incoords, userProfile.IsDisplayGoogleThumbnails, w3wResult), cancellationToken);
                        await Helpers.SendPushNotification(userProfile, "Mystery Point Generated", mesg);

                        CallbackOptions callbackOptions = new CallbackOptions()
                        {
                            StartTripReportDialog = true,
                            ShortCodes = new string[] { shortCode },
                            Messages = new string[] { mesg },
                            PointTypes = new PointTypes[] { PointTypes.MysteryPoint },
                            GeneratedPoints = ida,
                            NumWaterPointsSkipped = new int[] { numWaterPointsSkipped },
                            What3Words = new string[] { w3wResult.words },
                            NearestPlaces = new string[] { w3wResult.nearestPlace }
                        };
                        await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken, callbackOptions);
                    }, cancellationToken);
            });
        }
    }
}
