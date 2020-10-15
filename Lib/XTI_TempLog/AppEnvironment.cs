namespace XTI_TempLog
{
    public sealed class AppEnvironment
    {
        public AppEnvironment(string userName, string requesterKey, string remoteAddress, string userAgent, string versionKey)
        {
            UserName = userName;
            RequesterKey = requesterKey;
            RemoteAddress = remoteAddress;
            UserAgent = userAgent;
            VersionKey = versionKey;
        }

        public string UserName { get; }
        public string RequesterKey { get; }
        public string RemoteAddress { get; }
        public string UserAgent { get; }
        public string VersionKey { get; }
    }
}
