using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 23, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 1, VariableFieldCount = 1,
    VariableBlockStart = 1, MaxSize = 1677721600)]
public sealed class RequestAssetsPacket : INetworkSerializable
{
    public Asset[]? Assets { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Assets is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);

        if (Assets is not null)
        {
            writer.WriteVarInt(Assets.Length);

            foreach (var asset in Assets)
                asset.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Assets = new Asset[count];

            for (var i = 0; i < count; i++)
            {
                Assets[i] = reader.ReadObject<Asset>();
            }
        }
    }
}