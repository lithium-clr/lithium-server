using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ColorLightJsonConverter : JsonConverter<ColorLight>
{
    public override ColorLight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        var color = new ColorLight();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                return color;

            if (reader.TokenType is not JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");

            var propertyName = reader.GetString();
            reader.Read();

            var value = ReadByteValue(ref reader);

            switch (propertyName)
            {
                case "radius":
                    color.Radius = value;
                    break;
                case "red":
                    color.Red = value;
                    break;
                case "green":
                    color.Green = value;
                    break;
                case "blue":
                    color.Blue = value;
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    private static byte ReadByteValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType is JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var intValue))
                return (byte)Math.Clamp(intValue, 0, 255);
            
            if (reader.TryGetDouble(out var doubleValue))
                return (byte)Math.Clamp((int)Math.Round(doubleValue), 0, 255);
        }

        throw new JsonException($"Cannot convert {reader.TokenType} to byte");
    }

    public override void Write(Utf8JsonWriter writer, ColorLight value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("radius", value.Radius);
        writer.WriteNumber("red", value.Red);
        writer.WriteNumber("green", value.Green);
        writer.WriteNumber("blue", value.Blue);
        writer.WriteEndObject();
    }
}