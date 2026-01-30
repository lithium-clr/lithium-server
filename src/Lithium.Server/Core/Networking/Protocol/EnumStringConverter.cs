using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class EnumStringConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String &&
            Enum.TryParse<T>(reader.GetString(), true, out var parsed))
            return parsed;

        if (reader.TokenType is JsonTokenType.Number)
            return (T)Enum.ToObject(typeof(T), reader.GetByte());

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var key = reader.GetString();

        if (Enum.TryParse<T>(key, true, out var parsed))
            return parsed;

        throw new JsonException($"Invalid enum key '{key}' for {typeof(T).Name}");
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString());
}