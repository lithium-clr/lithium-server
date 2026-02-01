using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(Vector3FloatJsonConverter))]
public record struct Vector3Float : IVector<float>, INetworkSerializable
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
    [JsonPropertyName("z")] public float Z { get; set; }

    public static Vector3Float Zero => new();
    public static Vector3Float One => new();

    public Vector3Float()
    {
    }

    public Vector3Float(float all)
    {
        X = Y = Z = all;
    }

    public Vector3Float(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(X);
        writer.WriteFloat32(Y);
        writer.WriteFloat32(Z);
    }

    public void Deserialize(PacketReader reader)
    {
        X = reader.ReadFloat32();
        Y = reader.ReadFloat32();
        Z = reader.ReadFloat32();
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