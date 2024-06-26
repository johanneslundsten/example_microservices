using System.Net;
using ApplicationUtils;

namespace GrpcServiceWithDb;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, 5001, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });
            webBuilder.UseStartup<Startup>();
        })
        .ConfigureAppConfiguration((context, config) => config.AddQuestBackLogging(context))
        .ConfigureServices((context, services) => services.AddQuestBackOpenTelemetry(context))
        .ConfigureLogging((context, builder) => builder.AddQuestBackOpenTelemetryLogging(context));
}