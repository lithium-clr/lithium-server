using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(Vector2IntJsonConverter))]
public sealed class Vector2Int : IVector<int>, INetworkSerializable
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }

    public static Vector2Int Zero => new();
    public static Vector2Int One => new();

    public Vector2Int()
    {
    }

    public Vector2Int(int all)
    {
        X = Y = all;
    }

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(X);
        writer.WriteInt32(Y);
    }

    public void Deserialize(PacketReader reader)
    {
        X = reader.ReadInt32();
        Y = reader.ReadInt32();
    }
}