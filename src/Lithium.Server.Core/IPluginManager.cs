using System.Reflection;

namespace Lithium.Server.Core;

public interface IPluginManager
{
    List<Assembly> Assemblies { get; }
}