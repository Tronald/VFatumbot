using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
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
            if (command.StartsWith("/getattractor", StringComparison.InvariantCulture))
            {
                await AttractionActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
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
                await AttractionActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
            }
            else if (command.StartsWith("/getpseudo", StringComparison.InvariantCulture))
            {
                await PsuedoActionAsync(turnContext, userProfile, cancellationToken);
            }
            else if (command.StartsWith("/getquantum", StringComparison.InvariantCulture))
            {
                await QuantumActionAsync(turnContext, userProfile, cancellationToken);
            }
            else if (command.StartsWith("/getqtime", StringComparison.InvariantCulture))
            {
                await QuantumActionAsync(turnContext, userProfile, cancellationToken, true);
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
                await AttractionActionAsync(turnContext, userProfile, cancellationToken, mainDialog, true);
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
                await PointActionAsync(turnContext, userProfile, cancellationToken, mainDialog);
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
                            return;
                        }
                        if (inputtedRadius > Consts.RADIUS_MAX)
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Radius must be less than or equal to {Consts.RADIUS_MAX}m"), cancellationToken);
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
            }
        }

        public async Task AttractionActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
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
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)turnContext.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        int numWaterPointsSkipped = 0;

                    redo:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "attractor", idacou);
                        if (ida.Length > 0)
                        {
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
                                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
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

                                mesg = Tolog(turnContext, "attractor", ida[i]);
                                await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                if (numWaterPointsSkipped > 0)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped: " + numWaterPointsSkipped), cancellationToken);
                                }

                                await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                            }
                        }
                        else if (ida.Count() < 1)
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
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
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)turnContext.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        int numWaterPointsSkipped = 0;

                    redo:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "void", idacou);
                        if (ida.Length > 0)
                        {
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
                                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
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

                                mesg = Tolog(turnContext, "void", ida[i]);
                                await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                if (numWaterPointsSkipped > 0)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped: " + numWaterPointsSkipped), cancellationToken);
                                }

                                await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);

                                // TODO: tesing reporting experiences dialog
                                //await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, new TripReportDialog(null, null), cancellationToken);

                                //await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, new ReportDialog(), cancellationToken);
                                //await stepContext.BeginDialogAsync(nameof(TripReportDialog), null, cancellationToken);
                            }
                        }
                        else if (ida.Count() < 1)
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
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

            await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);
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
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)turnContext.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        int numWaterPointsSkipped = 0;

                    redo:
                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "any", idacou);
                        if (ida.Length > 0)
                        {
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
                                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
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

                                mesg = Tolog(turnContext, "ida", ida[i]);
                                await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                if (numWaterPointsSkipped > 0)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Number of water points skipped: " + numWaterPointsSkipped), cancellationToken);
                                }

                                await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                            }
                        }
                        else if (ida.Count() < 1)
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                        }
                    }, cancellationToken);
            });
        }

        public async Task IntentSuggestionActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. Quantumly randomizing the English dictionary for you."), cancellationToken);

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)turnContext.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string[] intentSuggestions = await Helpers.GetIntentSuggestionsAsync();
                        await turnContext.SendActivityAsync(MessageFactory.Text("Intent suggestions: " + string.Join(", ", intentSuggestions)), cancellationToken);
                        await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                    }, cancellationToken);
            });
        }

        public async Task QuantumActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, bool suggestTime = false)
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

            string mesg = Tolog(turnContext, "random", (float)incoords[0], (float)incoords[1], suggestTime ? "qtime" : "quantum");
            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

            await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);
        }

        public async Task PsuedoActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken)
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

            string mesg = Tolog(turnContext, "random", (float)incoords[0], (float)incoords[1], "pseudo");
            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

            await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);
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
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await turnContext.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)turnContext.Activity).GetConversationReference(),
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
                            for (int i = 0; i < idacou; i++)
                            {
                                var incoords = new double[] { att[i].X.center.point.latitude, att[i].X.center.point.longitude };
                                if (!userProfile.IsIncludeWaterPoints && await Helpers.IsWaterCoordinatesAsync(incoords))
                                {
                                    numAttWaterPointsSkipped++;
                                }
                                else
                                {
                                    mesg = Tolog(turnContext, "attractor", att[i]);
                                    await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                                    dynamic w3wResult1 = await Helpers.GetWhat3WordsAddressAsync(incoords);
                                    await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult1), cancellationToken);
                                }

                                incoords = new double[] { voi[i].X.center.point.latitude, voi[i].X.center.point.longitude };
                                if (!userProfile.IsIncludeWaterPoints && await Helpers.IsWaterCoordinatesAsync(incoords))
                                {
                                    numVoiWaterPointsSkipped++;
                                }
                                else
                                {
                                    mesg = Tolog(turnContext, "void", voi[i]);
                                    await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                                    dynamic w3wResult2 = await Helpers.GetWhat3WordsAddressAsync(incoords);
                                    await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult2), cancellationToken);
                                }
                            }

                            if (numAttWaterPointsSkipped > 1)
                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of attractor water points skipped: " + numAttWaterPointsSkipped), cancellationToken);
                            if (numVoiWaterPointsSkipped > 1)
                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of void water points skipped: " + numVoiWaterPointsSkipped), cancellationToken);

                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                        }
                        else// if (ida.Count() < 1) // TODO: is this needed vs idacou > 1 if ?
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                        }
                    }, cancellationToken);
            });
        }

        public async Task PointActionAsync(ITurnContext turnContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                turnContext.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)turnContext.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";
                        double[] incoords = new double[2];
                        int numWaterPointsSkipped = 0;

                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, -1/*not used?*/);

                    redoAtt:
                        FinalAttractor[] att = SortIDA(ida, "attractor", 1);
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(new double[] { att[0].X.center.point.latitude, att[0].X.center.point.longitude });
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but attractor water points. Try again later."), cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of attractor water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoAtt;
                            }
                        }

                    redoVoid:
                        FinalAttractor[] voi = SortIDA(ida, "void", 1);
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(new double[] { voi[0].X.center.point.latitude, voi[0].X.center.point.longitude });
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but void water points. Try again later."), cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of void water points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoVoid;
                            }
                        }

                        ida = SortIDA(ida, "any", 1);

                    redoPsuedo:
                        double[] pcoords = GetPseudoRandom(userProfile.Location.latitude, userProfile.Location.longitude, userProfile.Radius);
                        if (!userProfile.IsIncludeWaterPoints)
                        {
                            var isWaterPoint = await Helpers.IsWaterCoordinatesAsync(pcoords);
                            if (isWaterPoint)
                            {
                                numWaterPointsSkipped++;

                                if (numWaterPointsSkipped > Consts.WATER_POINTS_SEARCH_MAX)
                                {
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but psuedo water points. Try again later."), cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of attractor psuedo points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoPsuedo;
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
                                    await turnContext.SendActivityAsync(MessageFactory.Text("Couldn't find anything but quantum water points. Try again later."), cancellationToken);
                                    return;
                                }

                                await turnContext.SendActivityAsync(MessageFactory.Text("Number of attractor quantum points skipped so far: " + numWaterPointsSkipped), cancellationToken);
                                goto redoQuantum;
                            }
                        }

                        Random rn = new Random();
                        int mtd = rn.Next(100);

                        if (mtd < 20)
                        {
                            incoords[0] = pcoords[0];
                            incoords[1] = pcoords[1];
                            mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "pseudo");
                        }
                        if ((mtd < 40) && (mtd > 20))
                        {
                            if (att.Count() > 0)
                            {
                                incoords[0] = att[0].X.center.point.latitude;
                                incoords[1] = att[0].X.center.point.longitude;
                                mesg = Tolog(turnContext, "blind", att[0]);
                            }
                            else
                            {
                                incoords[0] = pcoords[0];
                                incoords[1] = pcoords[1];
                                mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "pseudo");
                            }
                        }
                        if ((mtd < 60) && (mtd > 40))
                        {
                            incoords[0] = qcoords[0];
                            incoords[1] = qcoords[1];
                            mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "quantum");
                        }
                        if ((mtd < 80) && (mtd > 60))
                        {
                            if (voi.Count() > 0)
                            {
                                incoords[0] = voi[0].X.center.point.latitude;
                                incoords[1] = voi[0].X.center.point.longitude;
                                mesg = Tolog(turnContext, "blind", voi[0]);
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
                                mesg = Tolog(turnContext, "blind", ida[0]);
                            }
                            else
                            {
                                incoords[0] = qcoords[0];
                                incoords[1] = qcoords[1];
                                mesg = Tolog(turnContext, "blind", (float)incoords[0], (float)incoords[1], "quantum");
                            }
                        }

                        await turnContext.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                        dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                        await turnContext.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                        await ((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(context, mainDialog, cancellationToken);
                    }, cancellationToken);
            });
        }
    }
}
