using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ColorAlphaJsonConverter : JsonConverter<ColorAlpha>
{
    public override ColorAlpha Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        var colorAlpha = new ColorAlpha();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                return colorAlpha;

            if (reader.TokenType is not JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");

            var propertyName = reader.GetString();
            reader.Read();

            var value = ReadSByteValue(ref reader);

            switch (propertyName)
            {
                case "alpha":
                    colorAlpha.Alpha = value;
                    break;
                case "red":
                    colorAlpha.Red = value;
                    break;
                case "green":
                    colorAlpha.Green = value;
                    break;
                case "blue":
                    colorAlpha.Blue = value;
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    private static sbyte ReadSByteValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType is JsonTokenType.Number)
        {
            if (reader.TryGetSByte(out var value))
                return value;
        }

        throw new JsonException($"Cannot convert {reader.TokenType} to byte");
    }


    public override void Write(Utf8JsonWriter writer, ColorAlpha value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("alpha", value.Alpha);
        writer.WriteNumber("red", value.Red);
        writer.WriteNumber("green", value.Green);
        writer.WriteNumber("blue", value.Blue);
        writer.WriteEndObject();
    }
}