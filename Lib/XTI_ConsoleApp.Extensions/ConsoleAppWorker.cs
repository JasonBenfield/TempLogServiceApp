using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_Schedule.Hosting;

namespace XTI_ConsoleApp.Extensions
{
    public sealed class ConsoleAppWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly ConsoleAppOptions options;

        public ConsoleAppWorker(IServiceProvider sp, IOptions<ConsoleAppOptions> options)
        {
            this.sp = sp;
            this.options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var worker = new AppWorker(sp, options.ImmediateActions, options.ScheduledActions, options.AlwaysRunningActions);
            var tasks = worker.Start(stoppingToken);
            await Task.WhenAll(tasks);
            var lifetime = sp.GetService<IHostApplicationLifetime>();
            lifetime.StopApplication();
        }
    }
}
