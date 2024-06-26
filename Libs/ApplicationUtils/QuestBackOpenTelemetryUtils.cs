using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ApplicationUtils;

public static class QuestBackOpenTelemetryUtils
{
    public static void AddQuestBackOpenTelemetry(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddOpenTelemetry().ConfigureResource(resource => resource
                .AddService(serviceName: context.HostingEnvironment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter());
    }
    
    public static void AddQuestBackOpenTelemetryLogging(this ILoggingBuilder builder, HostBuilderContext context)
    {
        builder.AddOpenTelemetry(options =>
        {
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(context.HostingEnvironment.ApplicationName);

            options.SetResourceBuilder(resourceBuilder)
                .AddConsoleExporter();
        });
    }
}