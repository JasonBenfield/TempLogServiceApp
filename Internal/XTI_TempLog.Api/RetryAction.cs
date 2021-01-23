using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;

namespace XTI_TempLog.Api
{
    public sealed class RetryAction : OptionalAction<EmptyRequest, EmptyActionResult>
    {
        private readonly TempLogs tempLogs;
        private readonly Clock clock;
        private readonly LogOptions options;

        public RetryAction(TempLogs tempLogs, Clock clock, IOptions<LogOptions> options)
        {
            this.tempLogs = tempLogs;
            this.clock = clock;
            this.options = options.Value;
        }

        public Task<bool> IsOptional()
        {
            var filesInProgress = getFilesInProgress();
            var isOptional = !filesInProgress.Any();
            return Task.FromResult(isOptional);
        }

        private static readonly string processingExtension = ".processing";

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            var filesInProgress = getFilesInProgress();
            foreach (var file in filesInProgress)
            {
                file.WithNewName(file.Name.Remove(file.Name.Length - processingExtension.Length));
            }
            return Task.FromResult(new EmptyActionResult());
        }

        private IEnumerable<ITempLogFile> getFilesInProgress()
        {
            var modifiedBefore = clock.Now().AddMinutes(-options.ProcessMinutesBefore);
            var logs = tempLogs.Logs();
            return logs.SelectMany(l => l.ProcessingFiles(modifiedBefore)).ToArray();
        }
    }
}
