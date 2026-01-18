using System.Buffers;

namespace Lithium.Server.Core.Protocol;

public interface IPacket
{
    int Id { get; }
    int ComputedSize { get; }

    void Serialize(IBufferWriter<byte> writer);
}