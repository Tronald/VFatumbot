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
            DispatchWorkerThread((object sender, DoWorkEventArgs e) =>
            {
                new DiscordBot().RunBotAsync().GetAwaiter().GetResult();
            });
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
