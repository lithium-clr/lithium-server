using System.Buffers;

namespace Lithium.Server.Core.Protocol;

public interface IPacket
{
    static int PacketId { get; }
    static int ComputedSize { get; }
    static int NullableBitFieldSize { get; }
    static int FixedBlockSize { get; }
    static int VariableFieldCount { get; }
    static int VariableBlockStart { get; }
    static int MaxSize { get; }
    static bool IsCompressed { get; }
    
    void Serialize(IBufferWriter<byte> writer);
}

public interface IPacket<out T> : IPacket where T : IPacket<T>
{
    static abstract T Deserialize(ReadOnlySpan<byte> span);
}