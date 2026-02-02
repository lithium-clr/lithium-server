using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Json.Serialization;

public sealed class EnumKeyDictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
    where TKey : struct, Enum
{
    public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dictionary = new Dictionary<TKey, TValue>();

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
            if (propertyName == null || !Enum.TryParse(propertyName, true, out TKey key))
            {
                throw new JsonException($"Unable to convert \"{propertyName}\" to Enum {typeof(TKey).Name}.");
            }

            reader.Read();
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            if (value != null)
            {
                dictionary[key] = value;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options)
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