using MainDB.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SessionLogWebApp.Client;
using System.Net.Http;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_AuthenticatorClient.Extensions;
using XTI_Configuration.Extensions;
using XTI_Core;
using XTI_Secrets.Extensions;
using XTI_TempLog;
using XTI_TempLog.Abstractions;
using XTI_TempLog.Api;
using XTI_TempLog.Extensions;
using XTI_WebAppClient;

namespace TempLogTool
{
    class Program
    {
        static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.UseXtiConfiguration(hostingContext.HostingEnvironment, args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();
                    services.AddXtiDataProtection();
                    services.AddAppDbContextForSqlServer(hostContext.Configuration);
                    services.Configure<LogOptions>(hostContext.Configuration.GetSection(LogOptions.Log));
                    services.AddScoped<AppFactory>();
                    services.AddScoped<Clock, UtcClock>();
                    services.Configure<AppOptions>(hostContext.Configuration.GetSection(AppOptions.App));
                    services.AddSingleton(_ => TempLogAppKey.AppKey);
                    services.AddSingleton<IAppApiUser, AppApiSuperUser>();
                    services.AddFileSecretCredentials();
                    services.AddAuthenticatorClientServices(hostContext.Configuration);
                    services.AddScoped(sp =>
                    {
                        var httpClientFactory = sp.GetService<IHttpClientFactory>();
                        var xtiToken = sp.GetService<IXtiToken>();
                        var appOptions = sp.GetService<IOptions<AppOptions>>().Value;
                        var env = sp.GetService<IHostEnvironment>();
                        var versionKey = env.IsProduction() ? "" : AppVersionKey.Current.Value;
                        return new SessionLogAppClient(httpClientFactory, xtiToken, appOptions.BaseUrl, versionKey);
                    });
                    services.AddScoped<IPermanentLogClient, PermanentLogClient>();
                    services.AddScoped<TempLogs>(sp =>
                    {
                        var dataProtector = sp.GetDataProtector("XTI_TempLog");
                        var hostEnv = sp.GetService<IHostEnvironment>();
                        var appDataFolder = new AppDataFolder()
                            .WithHostEnvironment(hostEnv);
                        return new DiskTempLogs(dataProtector, appDataFolder.Path(), "TempLogs");
                    });
                    services.AddScoped<AppApiFactory, TempLogApiFactory>();
                    services.AddScoped(sp => sp.GetService<AppApiFactory>().CreateForSuperUser());
                    services.AddScoped(sp => (TempLogApi)sp.GetService<IAppApi>());
                    services.AddHostedService<TempLogHostedService>();
                })
                .RunConsoleAsync();
        }
    }
}
