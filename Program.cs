using System.ComponentModel;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using VFatumbot.Discord;

namespace VFatumbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if RELEASE_PROD // no dev version yet
            // Run Discord bot
            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                new DiscordBot().RunBotAsync().GetAwaiter().GetResult();
            });
#endif

            // Run all other bots on the Bot Framework
            CreateWebHostBuilder(args).Build().Run();
        }

        public static void DispatchWorkerThread(DoWorkEventHandler handler)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += handler;
            backgroundWorker.RunWorkerAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((logging) =>
                {
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .UseStartup<Startup>();
    }
}
