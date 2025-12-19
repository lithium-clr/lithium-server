using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hytale.Server.Core;

public interface IEvent;

public sealed class EventSystem(IServiceProvider services, ILogger<EventSystem> logger, IPluginManager pluginManager)
{
    public void Post<TEvent>(Action<TEvent> invoke)
        where TEvent : class, IEvent
    {
        // using var scope = services.CreateScope();
        //
        // var handlers = scope.ServiceProvider.GetServices<TEvent>().ToList();
        // logger.LogInformation($"Found {handlers.Count} handlers for {typeof(TEvent).Name}");
        //
        // foreach (var handler in handlers)
        // {
        //     logger.LogInformation($"Invoking handler for {typeof(TEvent).Name}");
        //     invoke(handler);
        // }

        using var scope = services.CreateScope();

        foreach (var assembly in pluginManager.Assemblies)
        {
            logger.LogInformation("Looking assembly: " + assembly.FullName);
            
            var types = assembly.GetTypes()
                .Where(x => x is { IsAbstract: false, IsClass: true } && x.IsAssignableTo(typeof(Component)))
                .Where(x => x.IsAssignableTo(typeof(TEvent)))
                .Select(x => scope.ServiceProvider.GetService(x))
                .ToList();
            
            logger.LogInformation("Found {Count} components", types.Count);

            foreach (var type in types)
            {
                logger.LogInformation("Find component: " + type?.GetType().Name);
            }
        }
    }
}