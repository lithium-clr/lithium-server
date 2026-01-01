using System.Reflection;

namespace Lithium.Server.Core.Systems.Commands;

public sealed class ConsoleCommandRegistry
{
    private readonly Dictionary<string, ConsoleCommand> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, ConsoleCommand> Commands => _commands;

    public ConsoleCommandRegistry(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
            RegisterAssembly(assembly);
    }

    private void RegisterAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(
                         BindingFlags.Instance |
                         BindingFlags.Static |
                         BindingFlags.Public |
                         BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
                if (attr is null) continue;

                ValidateMethod(method, attr.Name);

                if (!_commands.TryAdd(attr.Name, new ConsoleCommand
                {
                    Name = attr.Name,
                    Description = attr.Description,
                    Method = method,
                    DeclaringType = method.IsStatic ? null : type,
                }))
                {
                    throw new InvalidOperationException(
                        $"Command '{attr.Name}' is already registered.");
                }
            }
        }
    }

    private static void ValidateMethod(MethodInfo method, string commandName)
    {
        if (method.ReturnType != typeof(void) &&
            !typeof(Task).IsAssignableFrom(method.ReturnType))
        {
            throw new InvalidOperationException(
                $"Command '{commandName}' must return void or Task.");
        }

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.ParameterType.IsByRef)
            {
                throw new InvalidOperationException(
                    $"Command '{commandName}' has ref/out parameter '{parameter.Name}'.");
            }
        }
    }

    public bool TryGet(string name, out ConsoleCommand command)
        => _commands.TryGetValue(name, out command!);
}
