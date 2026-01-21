using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol.Packets;

public readonly struct ServerInfoPacket(
    string? serverName,
    string? motd,
    int maxPlayers
) : IPacket<ServerInfoPacket>
{
    public static int Id => 223;
    private const int VariableBlockStart = 13;

    public readonly string? ServerName = serverName;
    public readonly string? Motd = motd;
    public readonly int MaxPlayers = maxPlayers;

    public static ServerInfoPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();
        var maxPlayers = reader.ReadInt32();

        var serverNameOffset = reader.ReadInt32();
        var motdOffset = reader.ReadInt32();

        var varBlock = buffer[VariableBlockStart..];

        string? serverName = null;

        if ((nullBits & 1) is not 0)
            serverName = PacketSerializer.ReadVarString(varBlock[serverNameOffset..], out _);

        string? motd = null;

        if ((nullBits & 2) is not 0)
            motd = PacketSerializer.ReadVarString(varBlock[motdOffset..], out _);

        return new ServerInfoPacket(serverName, motd, maxPlayers);
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;
        
        if (ServerName is not null) nullBits |= 1;
        if (Motd is not null) nullBits |= 2;

        stream.WriteByte(nullBits);

        Span<byte> maxPlayersBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(maxPlayersBuffer, MaxPlayers);
        stream.Write(maxPlayersBuffer);

        using var varBlockStream = new MemoryStream();
        var offsets = new int[2];

        WriteStr(ServerName, 0);
        WriteStr(Motd, 1);

        Span<byte> offsetBuffer = stackalloc byte[4];

        foreach (var offset in offsets)
        {
            BinaryPrimitives.WriteInt32LittleEndian(offsetBuffer, offset);
            stream.Write(offsetBuffer);
        }

        varBlockStream.Position = 0;
        varBlockStream.CopyTo(stream);

        return;

        void WriteStr(string? value, int index)
        {
            if (value is null)
            {
                offsets[index] = -1;
                return;
            }

            offsets[index] = (int)varBlockStream.Position;
            PacketSerializer.WriteVarString(varBlockStream, value);
        }
    }
}