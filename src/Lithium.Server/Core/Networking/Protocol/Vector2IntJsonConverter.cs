using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class Vector2IntJsonConverter : JsonConverter<Vector2Int>
{
    public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int x = 0, y = 0;

        if (reader.TokenType is not JsonTokenType.StartObject)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;

            var name = reader.GetString();
            reader.Read();

            switch (name)
            {
                case "x": x = reader.GetInt32(); break;
                case "y": y = reader.GetInt32(); break;
            }
        }

        return new Vector2Int(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2Int value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteEndObject();
    }
}