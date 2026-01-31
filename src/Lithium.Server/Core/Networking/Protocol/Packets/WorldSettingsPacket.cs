using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 20, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 5, VariableFieldCount = 1,
    VariableBlockStart = 5, MaxSize = 1677721600)]
public sealed class WorldSettingsPacket : INetworkSerializable
{
    public int WorldHeight { get; set; }
    public Asset[]? RequiredAssets { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (RequiredAssets is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteInt32(WorldHeight);

        if (RequiredAssets is not null)
        {
            writer.WriteVarInt(RequiredAssets.Length);

            foreach (var asset in RequiredAssets)
                asset.Serialize(writer);
        }
    }
}