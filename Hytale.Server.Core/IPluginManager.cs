using System.Reflection;

namespace Hytale.Server.Core;

public interface IPluginManager
{
    List<Assembly> Assemblies { get; }
}