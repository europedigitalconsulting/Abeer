using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Formatting.Compact;

using System;

namespace Abeer.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "SmartClick.ApiServer";
            try
            {
                Log.Information("Smart Click server starting");
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config)=>
                    config.WriteTo.Console()
                            .WriteTo.File("Abeer-server.log", rollingInterval: RollingInterval.Day))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
