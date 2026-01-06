using Lithium.Server.Core;

namespace Lithium.Server;

public interface IPluginRegistry
{
    T? Get<T>() where T : class, IComponent;
    IEnumerable<IComponent> All { get; }
}

public sealed class PluginRegistry : IPluginRegistry
{
    private readonly Dictionary<Type, IComponent> _plugins = [];

    public void Register(IComponent plugin)
    {
        _plugins[plugin.GetType()] = plugin;
    }

    public void Unregister(IComponent plugin)
    {
        _plugins.Remove(plugin.GetType());
    }

    public T? Get<T>() where T : class, IComponent => _plugins.TryGetValue(typeof(T), out var plugin)
        ? (T)plugin
        : null;

    public IEnumerable<IComponent> All => _plugins.Values;
}