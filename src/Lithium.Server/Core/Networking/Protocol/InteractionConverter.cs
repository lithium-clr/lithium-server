using System.Text.Json;
using Lithium.Server.Core.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionConverter : PolymorphicConverter<Interaction>
{
    protected override string DiscriminatorPropertyName => "typeId";

    protected override Dictionary<int, Type> DerivedTypes => new()
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

    protected override int DefaultTypeId => 1; // SimpleInteraction is a safer default than SimpleBlockInteraction

    protected override int InferTypeId(JsonElement root)
    {
        // ParallelInteraction has "next" as an array
        if (root.TryGetProperty("next", out var nextProp) && nextProp.ValueKind == JsonValueKind.Array)
            return 23;

        // SimpleBlockInteraction has "useLatestTarget"
        if (root.TryGetProperty("useLatestTarget", out _))
            return 0;

        // MemoriesConditionInteraction has "memoriesNext"
        if (root.TryGetProperty("memoriesNext", out _))
            return 43;

        // DamageEntityInteraction has "damageEffects" or "angledDamage"
        if (root.TryGetProperty("damageEffects", out _) || root.TryGetProperty("angledDamage", out _))
            return 21;

        return DefaultTypeId;
    }
}