using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 24, NullableBitFieldSize = 0, FixedBlockSize = 4, VariableFieldCount = 1, VariableBlockStart = 4,
    MaxSize = 2121)]
public sealed class AssetInitializePacket : INetworkSerializable
{
    public int Size { get; set; }
    public Asset Asset { get; set; } = new();

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(Size);
        Asset.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        Size = reader.ReadInt32();
        Asset.Deserialize(reader);
    }
}