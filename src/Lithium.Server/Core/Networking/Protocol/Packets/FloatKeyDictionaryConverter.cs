using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public class FloatKeyDictionaryConverter<TValue> : JsonConverter<Dictionary<float, TValue>>
{
    public override Dictionary<float, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dict = new Dictionary<float, TValue>();

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return dict;

            // clé en string -> float
            var keyStr = reader.GetString()!;
            if (!float.TryParse(keyStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var key))
                throw new JsonException($"Invalid float key: {keyStr}");

            reader.Read();
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
            dict.Add(key, value);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<float, TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            // clé au format "0.0"
            writer.WritePropertyName(kvp.Key.ToString("0.0", CultureInfo.InvariantCulture));
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }
        writer.WriteEndObject();
    }
}