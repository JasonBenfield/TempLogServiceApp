using System;
using XTI_App.Api;

namespace XTI_TempLog.Api
{
    public sealed class TempLogApiFactory : AppApiFactory
    {
        private readonly IServiceProvider services;

        public TempLogApiFactory(IServiceProvider services)
        {
            this.services = services;
        }

        protected override IAppApi _Create(IAppApiUser user) => new TempLogApi
        (
            user,
            services
        );
    }
}
