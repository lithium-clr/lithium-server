using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class Vector3FloatJsonConverter : JsonConverter<Vector3Float>
{
    public override Vector3Float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0, y = 0, z = 0;

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
                case "x": x = reader.GetSingle(); break;
                case "y": y = reader.GetSingle(); break;
                case "z": z = reader.GetSingle(); break;
            }
        }

        return new Vector3Float(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3Float value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}