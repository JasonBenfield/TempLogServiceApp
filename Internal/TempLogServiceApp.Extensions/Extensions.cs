﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SessionLogWebApp.Client;
using System.Net.Http;
using XTI_App;
using XTI_App.Api;
using XTI_Core;
using XTI_ServiceApp.Extensions;
using XTI_TempLog;
using XTI_TempLog.Abstractions;
using XTI_TempLog.Api;
using XTI_TempLog.Extensions;
using XTI_WebAppClient;

namespace TempLogServiceApp.Extensions
{
    public static class Extensions
    {
        public static void AddTempLogServiceAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ => TempLogAppKey.AppKey);
            services.AddSingleton<IAppApiUser, AppApiSuperUser>();
            services.AddXtiServiceAppServices(configuration);
            services.AddScoped(sp =>
            {
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                var xtiToken = sp.GetService<XtiToken>();
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
            services.AddScoped(sp =>
            {
                var appKey = sp.GetService<AppKey>();
                var apiUser = sp.GetService<IAppApiUser>();
                var tempLogs = sp.GetService<TempLogs>();
                var permanentLogClient = sp.GetService<IPermanentLogClient>();
                var clock = sp.GetService<Clock>();
                return new TempLogApi(appKey, apiUser, tempLogs, permanentLogClient, clock);
            });
            services.AddScoped<AppApi>(sp => sp.GetService<TempLogApi>());
        }
    }
}