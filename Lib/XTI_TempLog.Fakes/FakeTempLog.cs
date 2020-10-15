using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XTI_TempLog.Fakes
{
    public sealed class FakeTempLog : TempLog
    {
        private readonly Dictionary<string, FakeTempLogFile> files = new Dictionary<string, FakeTempLogFile>();

        public string[] Files() => files.Keys.ToArray();

        protected override ITempLogFile CreateFile(string name)
        {
            var key = name.ToLower();
            if (!files.TryGetValue(key, out var file))
            {
                file = new FakeTempLogFile(name);
                files.Add(key, file);
            }
            return file;
        }

        protected override IEnumerable<string> FileNames(string pattern)
            => files.Keys.Where
            (
                key => new Regex(pattern.Replace("*", ".*"), RegexOptions.IgnoreCase).IsMatch(key)
            )
            .ToArray();
    }
}
