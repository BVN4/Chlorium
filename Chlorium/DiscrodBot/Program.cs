﻿using DiscrodBot.Bot.Reactions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using System;
using System.IO;
using System.Reflection;

namespace DiscrodBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Warn($"Started {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                Reaction.reactList = new System.Collections.Generic.List<Reaction>()
                {
                    new AnnoReaction(),
                    new YandexReaction()
                };
                Reaction.init();
                Logic.init();
                DiscordNet.Start();
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
      .UseNLog();
    }
}
