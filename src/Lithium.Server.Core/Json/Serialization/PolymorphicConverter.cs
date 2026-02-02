using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Json.Serialization;

public abstract class PolymorphicConverter<TBase> : JsonConverter<TBase> where TBase : class
{
    protected abstract string DiscriminatorPropertyName { get; }
    protected abstract Dictionary<int, Type> DerivedTypes { get; }
    protected virtual int DefaultTypeId => 0;

    public override TBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var typeId = DefaultTypeId;
        var hasTypeId = false;

        if (jsonDoc.RootElement.TryGetProperty(DiscriminatorPropertyName, out var discriminatorProp))
        {
            typeId = discriminatorProp.GetInt32();
            hasTypeId = true;
        }

        if (!hasTypeId)
        {
            typeId = InferTypeId(jsonDoc.RootElement);
        }

        if (!DerivedTypes.TryGetValue(typeId, out var derivedType))
            throw new JsonException($"Unknown type discriminator: {typeId}");

        return JsonSerializer.Deserialize(jsonDoc.RootElement.GetRawText(), derivedType, options) as TBase;
    }

    protected virtual int InferTypeId(JsonElement root)
    {
        return DefaultTypeId;
    }

    public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
