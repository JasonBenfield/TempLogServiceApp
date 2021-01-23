using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class LogActionFactory
    {
        private readonly IServiceProvider services;

        public LogActionFactory(IServiceProvider services)
        {
            this.services = services;
        }

        public AppAction<EmptyRequest, EmptyActionResult> CreateMoveToPermanent()
        {
            var tempLogs = services.GetService<TempLogs>();
            var client = services.GetService<IPermanentLogClient>();
            var clock = services.GetService<Clock>();
            var options = services.GetService<IOptions<LogOptions>>();
            return new MoveToPermanentAction(tempLogs, client, clock, options);
        }

        public AppAction<EmptyRequest, EmptyActionResult> CreateRetry()
        {
            var tempLogs = services.GetService<TempLogs>();
            var clock = services.GetService<Clock>();
            var options = services.GetService<IOptions<LogOptions>>();
            return new RetryAction(tempLogs, clock, options);
        }
    }
}
