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
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
                if (attr is null) continue;

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
            Console.WriteLine(
                $"Command '{commandName}' must return {typeof(void)} or {typeof(Task)}");
        }

        foreach (var p in method.GetParameters())
        {
            if (!IsBindable(p.ParameterType))
            {
                var supportedTypes = string.Join(", ", GetSupportedTypes().Select(x => x.Name));

                Console.WriteLine(
                    $"Command '{commandName}' has unsupported parameter type '{p.ParameterType.Name}'. Supported types: {supportedTypes}");
            }
        }
    }

    private static IEnumerable<Type> GetSupportedTypes()
    {
        var asm = Assembly.GetExecutingAssembly();
        return asm.GetTypes().Where(x => x.IsPrimitive || x.IsEnum || x == typeof(string) || x == typeof(decimal));
    }

    private static bool IsSupportedType(Type type)
    {
        return GetSupportedTypes().Contains(type);
    }

    private static bool IsBindable(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return IsSupportedType(type);
    }

    public bool TryGet(string name, out ConsoleCommand command)
        => _commands.TryGetValue(name, out command!);
}