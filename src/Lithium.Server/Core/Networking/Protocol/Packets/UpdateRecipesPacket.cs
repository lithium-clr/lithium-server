using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 60,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 2,
    VariableBlockStart = 10,
    MaxSize = 1677721600
)]
public sealed class UpdateRecipesPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("recipes")]
    public Dictionary<string, CraftingRecipe>? Recipes { get; set; }

    [JsonPropertyName("removedRecipes")]
    public string[]? RemovedRecipes { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Recipes is not null) bits.SetBit(1);
        if (RemovedRecipes is not null) bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        var recipesOffsetSlot = writer.ReserveOffset();
        var removedRecipesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Recipes is not null)
        {
            writer.WriteOffsetAt(recipesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Recipes.Count);
            foreach (var (key, value) in Recipes)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(recipesOffsetSlot, -1);
        }

        if (RemovedRecipes is not null)
        {
            writer.WriteOffsetAt(removedRecipesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(RemovedRecipes.Length);
            foreach (var item in RemovedRecipes)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }
        else
        {
            writer.WriteOffsetAt(removedRecipesOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<UpdateType>();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Recipes = reader.ReadDictionaryAt(
                offsets[0],
                r => r.ReadUtf8String(),
                r =>
                {
                    var recipe = new CraftingRecipe();
                    recipe.Deserialize(r);
                    return recipe;
                }
            );
        }

        if (bits.IsSet(2))
        {
            RemovedRecipes = reader.ReadArrayAt(offsets[1], r => r.ReadUtf8String());
        }
    }
}