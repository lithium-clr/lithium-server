using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Json.Serialization;

public class IntKeyDictionaryConverter<TValue> : JsonConverter<Dictionary<int, TValue>>
{
    public override Dictionary<int, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dictionary = new Dictionary<int, TValue>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            if (!int.TryParse(propertyName, out var key))
            {
                throw new JsonException($"Unable to convert \"{propertyName}\" to an integer.");
            }

            reader.Read();
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            if (value != null)
            {
                dictionary.Add(key, value);
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<int, TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var (key, val) in value)
        {
            writer.WritePropertyName(key.ToString());
            JsonSerializer.Serialize(writer, val, options);
        }

        writer.WriteEndObject();
    }
}
