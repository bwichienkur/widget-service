using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;

namespace EDDY.IS.WidgetProvider.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger logger = null;

            try
            {
                logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
                logger.Info("Starting Ad Reporting Service...");

                CreateHostBuilder(args).Build().Run();

            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Error(ex, "Error starting application.");
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    });
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseNLog();
                });
    }
}
