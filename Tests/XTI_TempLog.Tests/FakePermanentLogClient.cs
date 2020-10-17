using System.Collections.Generic;
using System.Threading.Tasks;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Tests
{
    public sealed class FakePermanentLogClient : IPermanentLogClient
    {
        private readonly List<IStartSessionModel> startSessions = new List<IStartSessionModel>();

        public Task StartSession(IStartSessionModel model)
        {
            startSessions.Add(model);
            return Task.CompletedTask;
        }

        public IStartSessionModel[] StartSessions() => startSessions.ToArray();
    }
}
