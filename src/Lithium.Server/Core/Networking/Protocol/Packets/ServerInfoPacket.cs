using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 223, NullableBitFieldSize = 1, FixedBlockSize = 5, VariableFieldCount = 2, VariableBlockStart = 13,
    MaxSize = 32768023)]
public sealed class ServerInfoPacket : INetworkSerializable
{
    public string? ServerName { get; set; }
    public string? Motd { get; set; }
    public int MaxPlayers { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ServerName is not null)
            bits.SetBit(1);

        if (Motd is not null)
            bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteInt32(MaxPlayers);

        var serverNameOffsetSlot = writer.ReserveOffset();
        var motdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (ServerName is not null)
        {
            writer.WriteOffsetAt(serverNameOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ServerName, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(serverNameOffsetSlot, -1);
        }

        if (Motd is not null)
        {
            writer.WriteOffsetAt(motdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Motd, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(motdOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        MaxPlayers = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
            ServerName = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            Motd = reader.ReadVarUtf8StringAt(offsets[1]);
    }
}