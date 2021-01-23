using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_TempLog.Api;

namespace TempLogSetupApp
{
    public sealed class HostedService : BackgroundService
    {
        private readonly IServiceProvider services;

        public HostedService(IServiceProvider services)
        {
            this.services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = services.CreateScope();
            var tempLogSetup = scope.ServiceProvider.GetService<TempLogSetup>();
            await tempLogSetup.Run();
        }
    }
}
