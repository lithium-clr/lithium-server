using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(Vector2FloatJsonConverter))]
public sealed class Vector2Float : IVector<float>, INetworkSerializable
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }

    public static Vector2Float Zero => new();
    public static Vector2Float One => new();

    public Vector2Float()
    {
    }

    public Vector2Float(float all)
    {
        X = Y = all;
    }

    public Vector2Float(float x, float y)
    {
        X = x;
        Y = y;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(X);
        writer.WriteFloat32(Y);
    }

    public void Deserialize(PacketReader reader)
    {
        X = reader.ReadFloat32();
        Y = reader.ReadFloat32();
    }
}