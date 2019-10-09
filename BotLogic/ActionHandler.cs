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

        public async Task AttractionActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
        {
            int idacou = 1;
            if (stepContext.Context.Activity.Text.Contains("["))
            {
                string[] buf = stepContext.Context.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                stepContext.Context.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)stepContext.Context.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";

                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "attractor", idacou);
                        if (ida.Length > 0)
                        {
                            for (int i = 0; i < ida.Count(); i++)
                            {
                                mesg = Tolog(stepContext.Context, "attractor", ida[i]);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                var incoords = new double[] { ida[i].X.center.point.latitude, ida[i].X.center.point.longitude };

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                await mainDialog.ContinueDialog(context, cancellationToken);
                            }
                        }
                        else if (ida.Count() < 1)
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await mainDialog.ContinueDialog(context, cancellationToken);
                        }
                    }, cancellationToken);
            });
        }

        public async Task VoidActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
        {
            int idacou = 1;
            if (stepContext.Context.Activity.Text.Contains("["))
            {
                string[] buf = stepContext.Context.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                stepContext.Context.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)stepContext.Context.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";

                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "void", idacou);
                        if (ida.Length > 0)
                        {
                            for (int i = 0; i < ida.Count(); i++)
                            {
                                mesg = Tolog(stepContext.Context, "void", ida[i]);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                var incoords = new double[] { ida[i].X.center.point.latitude, ida[i].X.center.point.longitude };

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                await mainDialog.ContinueDialog(context, cancellationToken);
                            }
                        }
                        else if (ida.Count() < 1)
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await mainDialog.ContinueDialog(context, cancellationToken);
                        }
                    }, cancellationToken);
            });
        }

        public async Task LocationActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your current radius is {userProfile.Radius}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your current location is {userProfile.Latitude},{userProfile.Longitude}"), cancellationToken);

            var incoords = new double[] { userProfile.Latitude, userProfile.Longitude };

            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

            await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);
        }

        public async Task AnomalyActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = true)
        {
            int idacou = 1;
            if (stepContext.Context.Activity.Text.Contains("["))
            {
                string[] buf = stepContext.Context.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                stepContext.Context.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)stepContext.Context.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";

                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        ida = SortIDA(ida, "any", idacou);
                        if (ida.Length > 0)
                        {
                            for (int i = 0; i < ida.Count(); i++)
                            {
                                mesg = Tolog(stepContext.Context, "ida", ida[i]);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                                var incoords = new double[] { ida[i].X.center.point.latitude, ida[i].X.center.point.longitude };

                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                                await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                await mainDialog.ContinueDialog(context, cancellationToken);
                            }
                        }
                        else if (ida.Count() < 1)
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                            await mainDialog.ContinueDialog(context, cancellationToken);
                        }
                    }, cancellationToken);
            });
        }

        public async Task RadiusActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken)
        {
            string command = "" + stepContext.Context.Activity.Text;
            if (command.Contains(" "))
            {
                string newRadiusStr = command.Replace(command.Substring(0, command.IndexOf(" ", StringComparison.CurrentCulture)), "");
                int Radius = 3000;
                if (Int32.TryParse(newRadiusStr, out Radius))
                {
                    userProfile.Radius = Radius;
                }
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your current radius is {userProfile.Radius}"), cancellationToken);
        }

        public async Task QuantumActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken, bool suggestTime = false)
        {
            double[] incoords = GetQuantumRandom(userProfile.Latitude, userProfile.Longitude, userProfile.Radius);

            string mesg = Tolog(stepContext.Context, "random", (float)incoords[0], (float)incoords[1], suggestTime ? "qtime" : "quantum");
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

            await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);
        }

        public async Task PsuedoActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken)
        {
            double[] incoords = GetPseudoRandom(userProfile.Latitude, userProfile.Longitude, userProfile.Radius);

            string mesg = Tolog(stepContext.Context, "random", (float)incoords[0], (float)incoords[1], "pseudo");
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

            await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);
        }

        public async Task PairActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog, bool doScan = false)
        {
            int idacou = 1;
            if (stepContext.Context.Activity.Text.Contains("["))
            {
                string[] buf = stepContext.Context.Activity.Text.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                Int32.TryParse(buf[1], out idacou);
                if (idacou < 1 || idacou > 20)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Incorrect value. Parameter should be a digit from 1 to 20."), cancellationToken);
                    return;
                }
            }

            if (doScan)
            {
                // TODO: implement this logic they had ... or check if it's okay to have multiple scans going
                //if (metasessions.ContainsKey(u) == true)
                //{ await Bot.SendTextMessageAsync(message.Chat.Id, "Your scanning session is already in progress."); }
                //else
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Generation may take from 5 to 15 minutes."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);
            }

            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                stepContext.Context.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)stepContext.Context.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                        string mesg = "";

                        FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, /*u* not used*/-1, doScan ? 1 : 0);
                        FinalAttractor[] att = SortIDA(ida, "attractor", idacou);
                        FinalAttractor[] voi = SortIDA(ida, "void", idacou);
                        if (att.Count() > voi.Count()) {
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
                                mesg = Tolog(stepContext.Context, "attractor", att[i]);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                                var incoords = new double[] { att[i].X.center.point.latitude, att[i].X.center.point.longitude };
                                dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);
                                await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

                                mesg = Tolog(stepContext.Context, "void", voi[i]);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
                                incoords = new double[] { voi[i].X.center.point.latitude, voi[i].X.center.point.longitude };
                                w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);
                                await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

								await mainDialog.ContinueDialog(context, cancellationToken);
							}
                        }
                        else if (ida.Count() < 1) // TODO: is this needed vs idacou > 1 if ?
                        {
                            mesg = "No Anomalies found at the moment. Try again later.";
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);
							await mainDialog.ContinueDialog(context, cancellationToken);
						}
                    }, cancellationToken);
            });
        }

        public async Task PointActionAsync(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken, MainDialog mainDialog)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Wait a minute. It will take a while."), cancellationToken);

			DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
			{
				stepContext.Context.Adapter.ContinueConversationAsync(Consts.APP_ID,
                    ((Microsoft.Bot.Schema.Activity)stepContext.Context.Activity).GetConversationReference(),
                    async (context, token) =>
                    {
                            string mesg = "";
                            double[] incoords = new double[2];

                            FinalAttractor[] ida = GetIDA(userProfile.Location, userProfile.Radius, -1/*not used?*/);

                            FinalAttractor[] att = SortIDA(ida, "attractor", 1);
                            FinalAttractor[] voi = SortIDA(ida, "void", 1);
                            ida = SortIDA(ida, "any", 1);
                            double[] pcoords = GetPseudoRandom(userProfile.Location.latitude, userProfile.Location.longitude, userProfile.Radius);
                            double[] qcoords = GetQuantumRandom(userProfile.Location.latitude, userProfile.Location.longitude, userProfile.Radius);

                            Random rn = new Random();
                            int mtd = rn.Next(100);

                            if (mtd < 20)
                            {
                                incoords[0] = pcoords[0];
                                incoords[1] = pcoords[1];
                                mesg = Tolog(stepContext.Context, "blind", (float)incoords[0], (float)incoords[1], "pseudo");
                            }
                            if ((mtd < 40) && (mtd > 20))
                            {
                                if (att.Count() > 0)
                                {
                                    incoords[0] = att[0].X.center.point.latitude;
                                    incoords[1] = att[0].X.center.point.longitude;
                                    mesg = Tolog(stepContext.Context, "blind", att[0]);
                                }
                                else
                                {
                                    incoords[0] = pcoords[0];
                                    incoords[1] = pcoords[1];
                                    mesg = Tolog(stepContext.Context, "blind", (float)incoords[0], (float)incoords[1], "pseudo");
                                }
                            }
                            if ((mtd < 60) && (mtd > 40))
                            {
                                incoords[0] = qcoords[0];
                                incoords[1] = qcoords[1];
                                mesg = Tolog(stepContext.Context, "blind", (float)incoords[0], (float)incoords[1], "quantum");
                            }
                            if ((mtd < 80) && (mtd > 60))
                            {
                                if (voi.Count() > 0)
                                {
                                    incoords[0] = voi[0].X.center.point.latitude;
                                    incoords[1] = voi[0].X.center.point.longitude;
                                    mesg = Tolog(stepContext.Context, "blind", voi[0]);
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
                                    mesg = Tolog(stepContext.Context, "blind", ida[0]);
                                }
                                else
                                {
                                    incoords[0] = qcoords[0];
                                    incoords[1] = qcoords[1];
                                    mesg = Tolog(stepContext.Context, "blind", (float)incoords[0], (float)incoords[1], "quantum");
                                }
                            }

                            await stepContext.Context.SendActivityAsync(MessageFactory.Text(mesg), cancellationToken);

                            dynamic w3wResult = await Helpers.GetWhat3WordsAddressAsync(incoords);

                            await stepContext.Context.SendActivityAsync(ReplyFactory.CreateLocationCardsReply(incoords, w3wResult), cancellationToken);

						    await mainDialog.ContinueDialog(context, cancellationToken);
					}, cancellationToken);
			});
		}

        public async Task ToggleWaterSkip(WaterfallStepContext stepContext, UserProfile userProfile, CancellationToken cancellationToken)
        {
            userProfile.IsIncludeWaterPoints = !userProfile.IsIncludeWaterPoints;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Water points will be " + (userProfile.IsIncludeWaterPoints ? "included" : "skipped")), cancellationToken);
        }

        public async Task SaveActionAsync(WaterfallStepContext stepContext, UserState userState, UserProfile userProfile, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Water points will be " + (userProfile.IsIncludeWaterPoints ? "included" : "skipped")), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current location is {userProfile.Latitude},{userProfile.Longitude}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Current radius is {userProfile.Radius}"), cancellationToken);

            // TODO: I don't think this is saving the way we want it to
            await userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);
        }
    }
}
