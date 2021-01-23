using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SessionLogWebApp.Client;
using System.Net.Http;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using XTI_Configuration.Extensions;
using XTI_ConsoleApp.Extensions;
using XTI_Core;
using XTI_TempLog;
using XTI_TempLog.Abstractions;
using XTI_TempLog.Api;
using XTI_TempLog.Extensions;
using XTI_WebAppClient;

namespace TempLogSetupApp
{
    class Program
    {
        public static Task Main(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.UseXtiConfiguration(context.HostingEnvironment, args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(_ => TempLogAppKey.AppKey);
                    services.AddSingleton<IAppApiUser, AppApiSuperUser>();
                    services.AddXtiConsoleAppServices(hostContext.Configuration);
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
                    services.AddScoped<AppFactory>();
                    services.AddScoped<TempLogSetup>();
                    services.AddScoped(sp =>
                    {
                        var apiUser = sp.GetService<IAppApiUser>();
                        return sp.GetService<TempLogApiFactory>().Create(apiUser);
                    });
                    services.AddScoped(sp => (TempLogApi)sp.GetService<IAppApi>());
                    services.AddHostedService<HostedService>();
                })
                .RunConsoleAsync();
    }
}
