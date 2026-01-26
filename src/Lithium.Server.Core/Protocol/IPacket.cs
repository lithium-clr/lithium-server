namespace Lithium.Server.Core.Protocol;

public interface IPacket
{
    int Id { get; }
    bool IsCompressed { get; }

    void Serialize(Stream stream) => throw new NotImplementedException();
}

public interface IPacket<out T> : IPacket where T : IPacket<T>
{
    new static abstract int Id { get; }
    new static virtual bool IsCompressed => false;

    // Default Interface Methods (DIM) mapping static to instance
    int IPacket.Id => T.Id;
    bool IPacket.IsCompressed => T.IsCompressed;
    
    static virtual T Deserialize(ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException();
    }
}
