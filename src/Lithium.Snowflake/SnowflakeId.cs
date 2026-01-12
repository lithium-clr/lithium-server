using IdGen;

namespace Lithium.Snowflake;

public readonly record struct SnowflakeId
{
    private readonly long _id;
    
    internal SnowflakeId(IdGenerator generator)
    {
        _id = generator.CreateId();
    }
    
    public SnowflakeId(long id)
    {
        _id = id;
    }

    public override string ToString()
    {
        return _id.ToString();
    }

    public static implicit operator long(SnowflakeId id) => id._id;
}