using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_TempLog.Api;

namespace TempLogTool
{
    public sealed class TempLogHostedService : IHostedService
    {
        private readonly IServiceProvider services;

        public TempLogHostedService(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = services.CreateScope();
            var api = scope.ServiceProvider.GetService<TempLogApi>();
            await api.Log.Retry.Execute(new EmptyRequest());
            await api.Log.MoveToPermanent.Execute(new EmptyRequest());
            var lifetime = scope.ServiceProvider.GetService<IHostApplicationLifetime>();
            lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
