using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_Schedule.Hosting;

namespace XTI_ServiceApp.Extensions
{
    public sealed class ServiceAppWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly ServiceAppOptions options;

        public ServiceAppWorker(IServiceProvider sp, IOptions<ServiceAppOptions> options)
        {
            this.sp = sp;
            this.options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var worker = new AppWorker
            (
                sp,
                options.ImmediateActions,
                options.ScheduledActions,
                options.AlwaysRunningActions
            );
            var tasks = worker.Start(stoppingToken);
            return Task.WhenAll(tasks);
        }
    }
}
