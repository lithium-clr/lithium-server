using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 32, NullableBitFieldSize = 0, FixedBlockSize = 4, VariableFieldCount = 0, VariableBlockStart = 4,
    MaxSize = 4)]
public sealed class ViewRadiusPacket : INetworkSerializable
{
    public int Value { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(Value);
    }

    public void Deserialize(PacketReader reader)
    {
        Value = reader.ReadInt32();
    }
}