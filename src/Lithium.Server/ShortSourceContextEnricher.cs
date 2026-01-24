using Serilog.Core;
using Serilog.Events;

namespace Lithium.Server;

public sealed class ShortSourceContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext) 
            && sourceContext is ScalarValue { Value: string fullName })
        {
            var shortName = fullName.Split('.').LastOrDefault() ?? fullName;
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ShortSourceContext", shortName));
        }
    }
}