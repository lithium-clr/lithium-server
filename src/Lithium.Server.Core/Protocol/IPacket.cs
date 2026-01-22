namespace Lithium.Server.Core.Protocol;

public interface IPacket
{
    static abstract int Id { get; }
    static virtual bool IsCompressed => false;
    
    void Serialize(Stream stream)
    {
        throw new NotImplementedException();
    }
}

public interface IPacket<out T> : IPacket where T : IPacket<T>
{
    static virtual T Deserialize(ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException();
    }
}