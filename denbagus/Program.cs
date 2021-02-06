using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DenBagus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                // .Enrich.FromLogContext()
                //.WriteTo.Console()
                //.WriteTo.File(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter(),
                //@"logs\log.json",
                //rollingInterval: RollingInterval.Day,
                //fileSizeLimitBytes: 10485760,
                //rollOnFileSizeLimit: true,
                //shared: true,
                //flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureLogging((hostingContext, logging) =>
                 {
                     logging.ClearProviders();
                     logging.AddConsole(options => options.IncludeScopes = true);
                     logging.AddDebug();
                 })
                .UseSerilog((hostingContext, loggerConfig) =>
                    loggerConfig.ReadFrom
                    .Configuration(hostingContext.Configuration)
                )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel()
                        .UseContentRoot(System.IO.Directory.GetCurrentDirectory())
                        .UseIIS()
                        .UseStartup<Startup>();
                });
    }
}
