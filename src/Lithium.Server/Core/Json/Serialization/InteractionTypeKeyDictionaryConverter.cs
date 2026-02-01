using System.Text.Json;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core.Json.Serialization;

public sealed class InteractionTypeKeyDictionaryConverter<TValue> : JsonConverter<Dictionary<InteractionType, TValue>>
{
    public override Dictionary<InteractionType, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var dictionary = new Dictionary<InteractionType, TValue>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var propertyName = reader.GetString();
            
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new JsonException("Invalid property name");
            }

            if (!Enum.TryParse<InteractionType>(propertyName, true, out var key))
            {
                throw new JsonException($"Unable to convert \"{propertyName}\" to enum {nameof(InteractionType)}");
            }

            reader.Read();
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

            // La valeur peut être null pour les types référence, on l'ajoute quand même.
            dictionary.Add(key, value!);
        }

        throw new JsonException("Expected EndObject token");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<InteractionType, TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var (key, val) in value)
        {
            // Correction : Appliquer la politique de nommage (ex: camelCase)
            var propertyName = key.ToString();
            var finalPropertyName = options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;

            writer.WritePropertyName(finalPropertyName);
            JsonSerializer.Serialize(writer, val, options);
        }

        writer.WriteEndObject();
    }
}