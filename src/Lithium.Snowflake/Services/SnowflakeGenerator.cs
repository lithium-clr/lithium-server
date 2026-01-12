using IdGen;

namespace Lithium.Snowflake.Services;

public sealed class SnowflakeGenerator : IIdGenerator
{
    private readonly IdGenerator _generator;

    public SnowflakeGenerator(int generatorId = 0)
    {
        var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var structure = new IdStructure(41, 10, 12);
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));
        
        _generator = new IdGenerator(generatorId, options);
    }

    public SnowflakeId CreateId() => new SnowflakeId(_generator);
}