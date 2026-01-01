using System.Reflection;

namespace Lithium.Server.Core.Commands;

public sealed class ConsoleCommandRegistry
{
    private readonly Dictionary<string, ConsoleCommand> _commands = new();

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
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
                if (attr == null)
                    continue;

                ValidateMethod(method, attr.Name);

                _commands[attr.Name] = new ConsoleCommand
                {
                    Name = attr.Name,
                    Description = attr.Description,
                    Method = method,
                    DeclaringType = type
                };
            }
        }
    }

    private static void ValidateMethod(MethodInfo method, string commandName)
    {
        if (method.ReturnType != typeof(void) &&
            !typeof(Task).IsAssignableFrom(method.ReturnType))
        {
            throw new InvalidOperationException(
                $"Command '{commandName}' must return void or Task");
        }

        foreach (var p in method.GetParameters())
        {
            if (!IsBindable(p.ParameterType))
            {
                throw new InvalidOperationException(
                    $"Command '{commandName}' has unsupported parameter type '{p.ParameterType.Name}'");
            }
        }
    }

    private static bool IsBindable(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type.IsEnum;
    }

    public bool TryGet(string name, out ConsoleCommand command)
        => _commands.TryGetValue(name, out command!);
}