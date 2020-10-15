using System.Threading.Tasks;

namespace XTI_TempLog
{
    public interface ITempLogFile
    {
        string Name { get; }
        Task Write(string contents);
        Task<string> Read();
    }
}
