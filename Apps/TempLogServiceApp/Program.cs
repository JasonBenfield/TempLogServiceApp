using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TempLogServiceApp.Extensions;
using XTI_Configuration.Extensions;

namespace TempLogServiceApp
{
    class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.UseXtiConfiguration(context.HostingEnvironment, args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTempLogServiceAppServices(hostContext.Configuration);
                })
                .UseWindowsService();
    }
}
