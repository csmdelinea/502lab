using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ToRefactor
{
    public static class ToRemoveExtensions
    {
        public static ILoggingBuilder AddToRemoveServices(this ILoggingBuilder loggingBuilder)
        {
            return loggingBuilder.AddLog4Net();
        }

        public static IServiceCollection AddToRemoveServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddLogging(options =>
            {
                options.AddLog4Net();
            });
        }
    }
}