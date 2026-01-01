namespace Lithium.Server.Core.Systems.Commands;

public sealed class ConsoleCommandExecutor(
    IServiceProvider services,
    CommandArgumentBinder binder
)
{
    public async Task ExecuteAsync(ConsoleCommand command, string[] args)
    {
        using var scope = services.CreateScope();

        var parameters = binder.Bind(
            command.Method.GetParameters(),
            args
        );

        if (parameters.Length != command.Method.GetParameters().Length)
            return;

        if (command.Method.IsStatic)
        {
            var result = command.Method.Invoke(null, parameters);
            if (result is Task task) await task;
        }
        else
        {
            var instance = scope.ServiceProvider.GetRequiredService(command.DeclaringType);

            var result = command.Method.Invoke(instance, parameters);
            if (result is Task task) await task;
        }
    }
}