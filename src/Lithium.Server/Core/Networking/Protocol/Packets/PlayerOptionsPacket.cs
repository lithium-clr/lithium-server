

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class PlayerOptionsPacket : IPacket<PlayerOptionsPacket>
{
    public static int Id => 33;

    public PlayerSkin? Skin { get; init; }

    public static PlayerOptionsPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        PlayerSkin? skin = null;

        if ((nullBits & 1) is not 0)
            skin = PlayerSkin.Deserialize(buffer[reader.Offset..], out _);

        return new PlayerOptionsPacket { Skin = skin };
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;

        if (Skin is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);
        Skin?.Serialize(stream);
    }
}