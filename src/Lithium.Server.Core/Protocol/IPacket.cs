namespace Lithium.Server.Core.Protocol;

public interface IPacket
{
    static virtual bool IsCompressed => false;
    
    void Serialize(Stream stream)
    {
        throw new NotImplementedException();
    }
}

public interface IPacket<out T> : IPacket where T : struct, IPacket<T>
{
    static abstract int Id { get; }
    
    static virtual T Deserialize(byte[] buffer)
    {
        throw new NotImplementedException();
    }
}