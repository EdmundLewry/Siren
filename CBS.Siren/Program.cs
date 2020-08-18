using CBS.Siren.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using System;
using Microsoft.Extensions.Configuration;

namespace CBS.Siren
{
    class Program
    {
        protected Program(){}
     
        public static void Main(string[] args)
        {
            LoggingManager.ConfigureLogging();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                LoggingManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>{
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureWebHostDefaults(webBuilder =>{
                    webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging => {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog();
        }
    }
}