// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace VFatumbot
{
    public class Program
    {
        //[DllImport("libAttract.dll", CallingConvention = CallingConvention.Cdecl)]
        //private extern static ulong getOptimizedDots(double areaRadiusM); //how many coordinates is needed for requested radius, optimized for performance on larger areas

        public static void Main(string[] args)
        {
            //Console.WriteLine("getOptimizedDots: " + getOptimizedDots(3000));
            CreateWebHostBuilder(args).Build().Run();
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
