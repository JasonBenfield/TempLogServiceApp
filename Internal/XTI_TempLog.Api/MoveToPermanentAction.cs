using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog.Abstractions;

namespace XTI_TempLog.Api
{
    public sealed class MoveToPermanentAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly TempLogs tempLogs;
        private readonly IPermanentLogClient permanentLogClient;
        private readonly Clock clock;

        public MoveToPermanentAction(TempLogs tempLogs, IPermanentLogClient permanentLogClient, Clock clock)
        {
            this.tempLogs = tempLogs;
            this.permanentLogClient = permanentLogClient;
            this.clock = clock;
        }

        public async Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            var modifiedBefore = clock.Now().AddMinutes(-1);
            var logs = tempLogs.Logs();
            var logBatch = await processBatch(modifiedBefore, logs);
            while (hasMoreToProcess(logBatch))
            {
                await permanentLogClient.LogBatch(logBatch);
                logBatch = await processBatch(modifiedBefore, logs);
            }
            return new EmptyActionResult();
        }

        private static bool hasMoreToProcess(LogBatchModel logBatch)
        {
            return logBatch.StartSessions.Any()
                || logBatch.StartRequests.Any()
                || logBatch.AuthenticateSessions.Any()
                || logBatch.LogEvents.Any()
                || logBatch.EndRequests.Any()
                || logBatch.EndSessions.Any();
        }

        private async Task<LogBatchModel> processBatch(DateTime modifiedBefore, IEnumerable<TempLog> logs)
        {
            var logBatch = new LogBatchModel();
            logBatch.StartSessions = await processFiles<StartSessionModel>(logs.SelectMany(l => l.StartSessionFiles(modifiedBefore)));
            logBatch.StartRequests = await processFiles<StartRequestModel>(logs.SelectMany(l => l.StartRequestFiles(modifiedBefore)));
            logBatch.AuthenticateSessions = await processFiles<AuthenticateSessionModel>(logs.SelectMany(l => l.AuthSessionFiles(modifiedBefore)));
            logBatch.LogEvents = await processFiles<LogEventModel>(logs.SelectMany(l => l.LogEventFiles(modifiedBefore)));
            logBatch.EndRequests = await processFiles<EndRequestModel>(logs.SelectMany(l => l.EndRequestFiles(modifiedBefore)));
            logBatch.EndSessions = await processFiles<EndSessionModel>(logs.SelectMany(l => l.EndSessionFiles(modifiedBefore)));
            return logBatch;
        }

        private async Task<T[]> processFiles<T>(IEnumerable<ITempLogFile> files)
        {
            var filesToProcess = startProcessing(files.Take(50)).ToArray();
            var models = await deserializeFiles<T>(filesToProcess);
            deleteFiles(filesToProcess);
            return models.ToArray();
        }

        private IEnumerable<ITempLogFile> startProcessing(IEnumerable<ITempLogFile> files)
            => files.Select(f => f.WithNewName($"{f.Name}.processing"));

        private async Task<IEnumerable<T>> deserializeFiles<T>(IEnumerable<ITempLogFile> files)
        {
            var deserialized = new List<T>();
            foreach (var file in files)
            {
                var content = await file.Read();
                var model = JsonSerializer.Deserialize<T>(content);
                deserialized.Add(model);
            }
            return deserialized;
        }

        private void deleteFiles(IEnumerable<ITempLogFile> files)
        {
            foreach (var file in files)
            {
                file.Delete();
            }
        }
    }
}
