using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApplicationUtils;

public static class QuestBackHostBuilderUtils
{
    
    public static void AddQuestBackLogging(this IConfigurationBuilder config, HostBuilderContext context)
    {
        var env = context.HostingEnvironment;

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}