using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using Serilog;

namespace LightingServiceTracker
{
    public class Worker : BackgroundService
    {
        private string serviceName => "LightingService";

        // Check every minute
        private int delaySeconds => 60;
        private int delayMS => delaySeconds * 1000;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Lighting tracer service starting");

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Lighting tracker service stopping");

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Get the lighting service process
                var lightingService = Process.GetProcessesByName(serviceName).FirstOrDefault();

                // For some reason the service isn't started.  It may be manually stopped by me or by Aura app
                if (lightingService == null)
                {
                    Log.Information("{serviceName} is not running. Waiting {delaySeconds} seconds", serviceName, delaySeconds);

                    await Task.Delay(delayMS, stoppingToken);
                    continue;
                }


                try
                {
                    // Get average CPU percentage over a few cycles
                    double pct = CpuPercent();

                    if (pct >= 10)
                    {
                        await RestartService();
                    }
                }
                catch (Exception ex)
                {
                    // The most likely error is that the service went offline while reading NextValue()
                    Log.Error(ex, "");
                }

                // Pause a minute before we start checking again
                await Task.Delay(delayMS, stoppingToken);
            }
        }

        /// <summary>
        /// Calculate the percentage CPU time this process is using
        /// </summary>
        private double CpuPercent()
        {
            int maxLoops = 100;
            double total = 0;


            // Get a PerformanceCounter for the processer
            using (var cpu = new PerformanceCounter("Process", "% Processor Time", serviceName, true))
            {
                // This will happen if the service is stopped
                if (cpu.CategoryName == null) return total;

                // Loop through a bunch of times to get an average of the CPU processor time
                for (int i = 0; i < maxLoops; i++)
                {
                    // Get the next CPU value; some may be 0 as the service won't always be using CPU time
                    total += cpu.NextValue();
                }
            }

            // Divide the total by the count to get the average
            total /= maxLoops;

            // Divide by the ProcessorCount since % Processor Time is per processor
            total /= Environment.ProcessorCount;

            return total;
        }

        /// <summary>
        /// Restart the LightingService
        /// </summary>
        private async Task RestartService()
        {
            Log.Information("Restarting {serviceName}", serviceName);

            using var sc = new ServiceController(serviceName);
            if (sc == null)
            {
                Log.Information("Unable to locate ServiceControler for {serviceName}", serviceName);
                return;
            }

            if (sc.Status == ServiceControllerStatus.Stopped ||
                sc.Status == ServiceControllerStatus.StopPending)
            {
                Log.Information("{serviceName} is currently stopping", serviceName);
                return;
            }

            sc.Stop();

            // Wait for the service to stop
            sc.WaitForStatus(ServiceControllerStatus.Stopped);

            // Pause for a few seconds to let things clean up
            await Task.Delay(10 * 1000);

            // Restart the process
            sc.Start();

            // Wait for the service to be flagged as started
            sc.WaitForStatus(ServiceControllerStatus.StartPending);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
