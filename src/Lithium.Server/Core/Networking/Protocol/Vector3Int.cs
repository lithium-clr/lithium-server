using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(Vector3IntJsonConverter))]
public record struct Vector3Int : IVector<int>, INetworkSerializable
{
    public int X;
    public int Y;
    public int Z;
    
    public static Vector3Int Zero => new();
    public static Vector3Int One => new();

    public Vector3Int()
    {
    }
    
    public Vector3Int(int all)
    {
        X = Y = Z = all;
    }
    
    public Vector3Int(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(X);
        writer.WriteInt32(Y);
        writer.WriteInt32(Z);
    }

    public void Deserialize(PacketReader reader)
    {
        X = reader.ReadInt32();
        Y = reader.ReadInt32();
        Z = reader.ReadInt32();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        FormattableString formattable = $"";
        return formattable.ToString(formatProvider);
    }

    public override string ToString()
    {
        return $"";
    }
}