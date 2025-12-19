using System.Reflection;
using Hytale.Server.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hytale.Server;

public sealed class PluginManager(
    IServiceProvider services,
    ILogger<PluginManager> logger,
    IPluginRegistry registry)
    : IPluginManager, IDisposable
{
    private const string PluginPath =
        @"C:\Users\bubbl\Desktop\Hytale\Build\Plugins\net10.0";

    private readonly PluginRegistry _registry = (PluginRegistry)registry;
    private readonly Dictionary<Assembly, IComponent> _instances = [];
    
    public List<Assembly> Assemblies { get; private set; } = [];

    public void LoadPlugins()
    {
        foreach (var dll in Directory.EnumerateFiles(PluginPath, "*.dll"))
        {
            LoadPlugin(dll);
        }
    }

    private void LoadPlugin(string path)
    {
        logger.LogInformation("Loading plugin {Path}", path);

        var assembly = Assembly.LoadFrom(path);
        var type = FindPluginType(assembly);

        if (type is null)
        {
            logger.LogDebug("Plugin {Path} does not contain a plugin type", path);
            return;
        }

        var plugin = (IComponent)ActivatorUtilities.CreateInstance(
            services,
            type);

        _instances[assembly] = plugin;
        Assemblies.Add(assembly);
        _registry.Register(plugin);

        plugin.OnLoad();

        logger.LogInformation("Loaded plugin {Plugin}", type.FullName);
    }

    private static Type? FindPluginType(Assembly assembly)
    {
        return assembly.GetTypes()
            .FirstOrDefault(t =>
                typeof(Component).IsAssignableFrom(t) &&
                t is { IsClass: true, IsAbstract: false });
    }

    void IDisposable.Dispose()
    {
        foreach (var plugin in _instances.Values)
        {
            plugin.OnUnload();
            _registry.Unregister(plugin);
        }
        
        _instances.Clear();
        Assemblies.Clear();
    }
}