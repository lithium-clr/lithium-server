using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class Vector3IntJsonConverter : JsonConverter<Vector3Int>
{
    public override Vector3Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int x = 0, y = 0, z = 0;

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
                case "z": z = reader.GetInt32(); break;
            }
        }

        return new Vector3Int(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3Int value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}