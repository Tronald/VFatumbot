// THIS FILE IS(WAS) A PART OF EMZI0767'S BOT EXAMPLES
//
// --------
// 
// Copyright 2017 Emzi0767
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// --------
//

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using VFatumbot.BotLogic;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VFatumbot.Discord
{
    public class TurnContextWrapper : ITurnContext
    {
        public TurnContextStateCollection TurnState => throw new NotImplementedException();
        public bool Responded => throw new NotImplementedException();
        public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            throw new NotImplementedException();
        }
        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            throw new NotImplementedException();
        }
        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            throw new NotImplementedException();
        }
        public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        //// ^^^^^ not implemented ^^^^^^^
        ///
        ///　↓↓↓↓↓↓  force implemented so we didn't have to modify lots of Bot Framework Fatumbot code just for Discord  ↓↓↓↓↓↓↓↓

        public async Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default)
        {
            return await SendActivityAsync(MessageFactory.Text(textReplyToSend), cancellationToken);
        }

        public async Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default)
        {
            var activity = activities[0];
            if (activity.Type == "DiscordEmbed")
            {
                await _ctx.RespondAsync(embed: ((Activity)activity).Entities[0].GetAs<DiscordEmbedBuilder>());
            }
            else
            {
                // Not used but maybe one day...
                await _ctx.RespondAsync(((Activity)activity).Text.Replace("\n\n", "\n"));
            }

            return new ResourceResponse[] { new ResourceResponse() };
        }

        public async Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        {
            await _ctx.RespondAsync(((Activity)activity).Text.Replace("\n\n", "\n"));

            return new ResourceResponse();
        }

        public BotAdapter Adapter => GetAdapter();
        public Activity Activity => GetActivity();

        CommandContext _ctx;
        string _command;

        public TurnContextWrapper(CommandContext ctx, int numPoints = 0)
        {
            _ctx = ctx;

            if (numPoints > 0)
            {
                _command = $"{_ctx.Message.Content.Trim()}[{numPoints}]";
            }
            else
            {
                _command = _ctx.Message.Content.Trim();
            }
        }

        private AdapterWithErrorHandlerWrapper GetAdapter()
        {
            var adapter = new AdapterWithErrorHandlerWrapper(this);
            return adapter;
        }

        private Activity GetActivity()
        {
            var activity = new Activity(text: _command,
                                        from: new ChannelAccount(id: _ctx.User.Id.ToString(), name: _ctx.User.Username),
                                        timestamp: _ctx.Message.Timestamp,
                                        channelId: Enums.ChannelPlatform.discord.ToString());
            return activity;
        }
    }

    public class AdapterWithErrorHandlerWrapper : AdapterWithErrorHandler
    {
        public ITurnContext _ctx;

        public AdapterWithErrorHandlerWrapper(ITurnContext turnContext) : base() {
            _ctx = turnContext;
        }

        public AdapterWithErrorHandlerWrapper(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, logger) {}

        public async override Task ContinueConversationAsync(string botAppId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
           await callback.Invoke(_ctx, cancellationToken);
        }
    }  

    public class DiscordCommandHandler
    {
        private CosmosDbStorage temporaryStorage = new CosmosDbStorage(new CosmosDbStorageOptions
        {
            AuthKey = Consts.COSMOS_DB_KEY,
            CollectionId = Consts.COSMOS_CONTAINER_NAME_TEMPORARY,
            CosmosDBEndpoint = new Uri(Consts.COSMOS_DB_URI),
            DatabaseId = Consts.COSMOS_DB_NAME,
        });

        private async Task<UserProfileTemporary> GetUserProfileTemporaryAsync(CommandContext ctx)
        {
            var key = "discord/users/" + ctx.User.Id;
            var userProfileTemporaries = await temporaryStorage.ReadAsync<UserProfileTemporary>(new string[] { key });
            UserProfileTemporary userProfileTemporary = null;
            userProfileTemporaries.TryGetValue(key, out userProfileTemporary);
            if (userProfileTemporary == null)
                userProfileTemporary = new UserProfileTemporary();
            return userProfileTemporary;
        }

        private async Task SaveUserProfileTemporaryAsync(CommandContext ctx, UserProfileTemporary userProfileTemporary)
        {
            var key = "discord/users/" + ctx.User.Id;
            var dict = new Dictionary<string, object>();
            userProfileTemporary.UserId = ctx.User.Id.ToString();
            dict.Add(key, userProfileTemporary);
            await temporaryStorage.WriteAsync(dict, new CancellationToken());
        }

        [Command("getattractor"), Description("Get an attractor point")]
        [Aliases("scanattractor")]
        public async Task GetAttractor(CommandContext ctx, [Description("Number of attractor points to generate")] params int[] numberPoints)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx, numberPoints.Length == 1 ? numberPoints[0] : 1);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getvoid"), Description("Get a void point")]
        [Aliases("scanvoid")]
        public async Task GetVoid(CommandContext ctx, [Description("Number of void points to generate")] params int[] numberPoints)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx, numberPoints.Length == 1 ? numberPoints[0] : 1);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getintents"), Description("Get intent suggestions")]
        public async Task GetAnomaly(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getpair"), Description("Get a pair of attractor and void points")]
        [Aliases("scanpair")]
        public async Task GetPair(CommandContext ctx, [Description("Number of pairs to generate")] params int[] numberPoints)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx, numberPoints.Length == 1 ? numberPoints[0] : 1);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getpseudo"), Description("Get a single pseudo random point")]
        public async Task GetPseudo(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getquantum"), Description("Get a single quantum random point")]
        public async Task GetQuantum(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getqtime"), Description("Get a single quantum random point with suggested time to visit")]
        public async Task GetQTime(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getanomaly"), Description("Get a pair of attractor and void points")]
        [Aliases("getida", "scananomaly","scanida")]
        public async Task GetAnomaly(CommandContext ctx, [Description("Number of points to generate")] params int[] numberPoints)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx, numberPoints.Length == 1 ? numberPoints[0] : 1);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("getpoint"), Description("Get a mystery point")]
        [Aliases("mysterypoint")]
        public async Task GetPoint(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("myrandotrips"), Description("Get all your randotrips")]
        public async Task GetMyRandotrip(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("randotrips"), Description("Get today's randotrip")]
        public async Task GetRandotrips(CommandContext ctx, [Description("yyyy-mm-dd")] params string[] date)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("setradius"), Description("Set radius")]
        public async Task SetRadius(CommandContext ctx, [Description("Radius in meters")] params string[] radius)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
            await SaveUserProfileTemporaryAsync(ctx, userProfileTemporary);
        }

        [Command("setlocation"), Description("Set location")]
        public async Task SetLocation(CommandContext ctx, [Description("Address/place name or a Google Maps URL")] params string[] location)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
            await SaveUserProfileTemporaryAsync(ctx, userProfileTemporary);
        }

        [Command("mylocation"), Description("Get your currently set location")]
        public async Task MyLocation(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("togglewater"), Description("Toggle skipping water points")]
        public async Task ToggleWater(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
            await SaveUserProfileTemporaryAsync(ctx, userProfileTemporary);
        }

        [Command("morehelp"), Description("Get more help")]
        public async Task Help(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }

        [Command("test"), Description("Check if the bot and QRNG are online")]
        public async Task Test(CommandContext ctx)
        {
            var userProfileTemporary = await GetUserProfileTemporaryAsync(ctx);
            var handler = new ActionHandler();
            var turnContextWrapper = new TurnContextWrapper(ctx);
            await handler.ParseSlashCommands(turnContextWrapper, userProfileTemporary, new CancellationToken(), null);
        }
    }
}
