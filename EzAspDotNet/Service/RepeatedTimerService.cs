using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EzAspDotNet.Service
{
    public class RepeatedTimerService : IHostedService, IDisposable
    {
        private Timer _timer;

        private TimeSpan _interval;

        public RepeatedTimerService(TimeSpan interval)
        {
            _interval = interval;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, _interval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("Timed Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        protected virtual void DoWork(object state)
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
