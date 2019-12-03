using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VFatumbot.BotLogic;
using Microsoft.Bot.Connector.Authentication;

namespace VFatumbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetry();

            // Create the telemetry client.
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();

            // Add telemetry initializer that will set the correlation context for all telemetry items.
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();

            // Add telemetry initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID)
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();

            // Create the telemetry middleware to initialize telemetry gathering
            services.AddSingleton<IMiddleware, TelemetryInitializerMiddleware>();

            // Create the telemetry middleware (used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // For the bot running in the Azure cloud, we need to use Cosmos DB (or Azure's Blob Storage service)
            // to keep data persistent, otherwise the stateless nature of the bot would be useless in keeping
            // track of users's locations, radius settings etc.
            var persistentStorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = Consts.COSMOS_DB_KEY,
                CollectionId = Consts.COSMOS_CONTAINER_NAME_PERSISTENT,
                CosmosDBEndpoint = new Uri(Consts.COSMOS_DB_URI),
                DatabaseId = Consts.COSMOS_DB_NAME,
            });

            var temporaryStorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = Consts.COSMOS_DB_KEY,
                CollectionId = Consts.COSMOS_CONTAINER_NAME_TEMPORARY,
                CosmosDBEndpoint = new Uri(Consts.COSMOS_DB_URI),
                DatabaseId = Consts.COSMOS_DB_NAME,
            });

            var conversationState = new ConversationState(persistentStorage);
            var userPersistentState = new UserPersistentState(persistentStorage);
            var userTemporaryState = new UserTemporaryState(temporaryStorage);

            // Add the states as singletons
            services.AddSingleton(conversationState);
            services.AddSingleton(userPersistentState);
            services.AddSingleton(userTemporaryState);

/*
            //
            // In-mem only way
            //
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state.
            services.AddSingleton<UserPersistentState>();
            services.AddSingleton<UserTemporaryState>();

            // Create the Conversation state.
            services.AddSingleton<ConversationState>();
*/

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, VFatumbot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                // for serving KML randotrip files
                ServeUnknownFileTypes = true,
            });

            app.UseMvc();
        }
    }

    public class ConfigurationCredentialProvider : SimpleCredentialProvider
    {
        public ConfigurationCredentialProvider(IConfiguration configuration)
            : base(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"])
        {
        }
    }
}
