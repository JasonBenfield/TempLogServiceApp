using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class MoveToPermanentAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly TempLog tempLog;
        private readonly IPermanentLogClient permanentLogClient;

        public MoveToPermanentAction(TempLog tempLog, IPermanentLogClient permanentLogClient)
        {
            this.tempLog = tempLog;
            this.permanentLogClient = permanentLogClient;
        }

        public async Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            var startSessionFiles = tempLog.StartSessionFiles();
            foreach (var startSessionFile in startSessionFiles)
            {
                var content = await startSessionFile.Read();
                var startSessionModel = JsonSerializer.Deserialize<StartSessionModel>(content);
                await permanentLogClient.StartSession(startSessionModel);
            }
            return new EmptyActionResult();
        }
    }
}
