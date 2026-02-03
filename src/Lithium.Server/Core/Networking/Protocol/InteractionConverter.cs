using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionConverter : JsonConverter<Interaction>
{
    private static readonly Dictionary<int, Type> IdToType = new()
    {
        { 0, typeof(SimpleBlockInteraction) },
        { 1, typeof(SimpleInteraction) },
        { 2, typeof(PlaceBlockInteraction) },
        { 3, typeof(BreakBlockInteraction) },
        { 4, typeof(PickBlockInteraction) },
        { 5, typeof(UseBlockInteraction) },
        { 6, typeof(UseEntityInteraction) },
        { 7, typeof(BuilderToolInteraction) },
        { 8, typeof(ModifyInventoryInteraction) },
        { 9, typeof(ChargingInteraction) },
        { 10, typeof(WieldingInteraction) },
        { 11, typeof(ChainingInteraction) },
        { 12, typeof(ConditionInteraction) },
        { 13, typeof(StatsConditionInteraction) },
        { 14, typeof(BlockConditionInteraction) },
        { 15, typeof(ReplaceInteraction) },
        { 16, typeof(ChangeBlockInteraction) },
        { 17, typeof(ChangeStateInteraction) },
        { 18, typeof(FirstClickInteraction) },
        { 20, typeof(SelectInteraction) },
        { 21, typeof(DamageEntityInteraction) },
        { 22, typeof(RepeatInteraction) },
        { 23, typeof(ParallelInteraction) },
        { 24, typeof(ChangeActiveSlotInteraction) },
        { 25, typeof(EffectConditionInteraction) },
        { 26, typeof(ApplyForceInteraction) },
        { 27, typeof(ApplyEffectInteraction) },
        { 28, typeof(ClearEntityEffectInteraction) },
        { 29, typeof(SerialInteraction) },
        { 30, typeof(ChangeStatInteraction) },
        { 31, typeof(MovementConditionInteraction) },
        { 32, typeof(ProjectileInteraction) },
        { 33, typeof(RemoveEntityInteraction) },
        { 34, typeof(ResetCooldownInteraction) },
        { 35, typeof(TriggerCooldownInteraction) },
        { 36, typeof(CooldownConditionInteraction) },
        { 37, typeof(ChainFlagInteraction) },
        { 38, typeof(IncrementCooldownInteraction) },
        { 39, typeof(CancelChainInteraction) },
        { 40, typeof(RunRootInteraction) },
        { 41, typeof(CameraInteraction) },
        { 42, typeof(SpawnDeployableFromRaycastInteraction) },
        { 43, typeof(MemoriesConditionInteraction) },
        { 44, typeof(ToggleGliderInteraction) }
    };

    private const int DefaultTypeId = 1;

    // =========================
    // WRITE
    // =========================

    public override void Write(Utf8JsonWriter writer, Interaction value, JsonSerializerOptions options)
    {
        // Sérialise le type runtime directement
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    // =========================
    // READ
    // =========================

    public override Interaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var typeId = InferTypeId(root);

        if (!IdToType.TryGetValue(typeId, out var targetType))
            targetType = IdToType[DefaultTypeId];

        var json = root.GetRawText();

        return (Interaction)JsonSerializer.Deserialize(json, targetType, options)!;
    }

    // =========================
    // TYPE INFERENCE (métier)
    // =========================

    private static int InferTypeId(JsonElement root)
    {
        // ParallelInteraction → next array
        if (root.TryGetProperty("next", out var nextProp) &&
            nextProp.ValueKind == JsonValueKind.Array)
            return 23;

        // SimpleBlockInteraction
        if (root.TryGetProperty("useLatestTarget", out _))
            return 0;

        // MemoriesConditionInteraction
        if (root.TryGetProperty("memoriesNext", out _))
            return 43;

        // DamageEntityInteraction
        if (root.TryGetProperty("damageEffects", out _) ||
            root.TryGetProperty("angledDamage", out _))
            return 21;

        // fallback
        return DefaultTypeId;
    }
}
