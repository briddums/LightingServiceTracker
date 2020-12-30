using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace LightingServiceTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
            Timestamp: event's timestamp as a DateTimeOffset

            Level: log event level (Verbose, Debug, Information, Warning, Error, Fatal)
                   u# - uppercase first # letters
                    or
                   w3# - lowercase first # letters

            Message: Log's event message rendered as plain text.  :l removes quotes from strings (else string variables will be "quoted")

            NewLine: System.Enviroment.NewLine

            Exception: Full exception message & stack trace, formatted across multiple lines.  Empty if no exception

            Properties: All event properties that don't appear elsewhere in the log
            */
            var outputTemplate = "{Timestamp:HH:mm:ss}> {Message:l}{NewLine}{Exception}";
            var filePath = $"E:/Projects/LightingServiceTracker/logs/log-{DateTime.Now:yyyy-MM-dd}.txt";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(filePath,
                              outputTemplate: outputTemplate,
                              rollingInterval: RollingInterval.Day)
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
