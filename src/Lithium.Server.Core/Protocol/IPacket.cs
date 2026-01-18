using System.Buffers;

namespace Lithium.Server.Core.Protocol;

public interface IPacket
{
    static int PacketId { get; }
    static int ComputedSize { get; }
    
    void Serialize(IBufferWriter<byte> writer);
}

public interface IPacket<out T> : IPacket where T : IPacket<T>
{
    static abstract T Deserialize(ReadOnlySpan<byte> span);
}