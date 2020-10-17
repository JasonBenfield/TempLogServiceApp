using SessionLogWebApp.Api;
using System.Threading.Tasks;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Tests
{
    public sealed class PermanentLogClient : IPermanentLogClient
    {
        private readonly SessionLogApi sessionLogApi;

        public PermanentLogClient(SessionLogApi sessionLogApi)
        {
            this.sessionLogApi = sessionLogApi;
        }

        public Task StartSession(IStartSessionModel model)
            => sessionLogApi.PermanentLog.StartSession.Execute((StartSessionModel)model);
    }
}
