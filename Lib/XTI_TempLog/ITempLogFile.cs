using System;
using System.Threading.Tasks;

namespace XTI_TempLog
{
    public interface ITempLogFile
    {
        string Name { get; }
        DateTime LastModified { get; }
        Task Write(string contents);
        Task<string> Read();
        void StartProcessing();
        void Delete();
    }
}
