namespace Lithium.Snowflake.Services;

public interface IIdGenerator
{
    SnowflakeId CreateId();
}