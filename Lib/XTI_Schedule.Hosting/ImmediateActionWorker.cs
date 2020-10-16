using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_Schedule.Hosting
{
    public sealed class ImmediateActionWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly ImmediateActionOptions options;

        public ImmediateActionWorker(IServiceProvider sp, ImmediateActionOptions options)
        {
            this.sp = sp;
            this.options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = sp.CreateScope();
            var api = scope.ServiceProvider.GetService<AppApi>();
            var action = api
                .Group(options.GroupName)
                .Action<EmptyRequest, EmptyActionResult>(options.ActionName);
            await action.Execute(new EmptyRequest());
        }
    }
}
