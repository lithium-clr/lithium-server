using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 7,
    VariableFieldCount = 7,
    VariableBlockStart = 35,
    MaxSize = 114688070
)]
public sealed class ItemQuality : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("itemTooltipTexture")]
    public string? ItemTooltipTexture { get; set; }

    [JsonPropertyName("itemTooltipArrowTexture")]
    public string? ItemTooltipArrowTexture { get; set; }

    [JsonPropertyName("slotTexture")] public string? SlotTexture { get; set; }
    [JsonPropertyName("blockSlotTexture")] public string? BlockSlotTexture { get; set; }

    [JsonPropertyName("specialSlotTexture")]
    public string? SpecialSlotTexture { get; set; }

    [JsonPropertyName("textColor")] public Color? TextColor { get; set; }
    [JsonPropertyName("localizationKey")] public string? LocalizationKey { get; set; }

    [JsonPropertyName("visibleQualityLabel")]
    public bool VisibleQualityLabel { get; set; }

    [JsonPropertyName("renderSpecialSlot")]
    public bool RenderSpecialSlot { get; set; }

    [JsonPropertyName("hideFromSearch")] public bool HideFromSearch { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (TextColor is not null) bits.SetBit(1);
        if (Id is not null) bits.SetBit(2);
        if (ItemTooltipTexture is not null) bits.SetBit(4);
        if (ItemTooltipArrowTexture is not null) bits.SetBit(8);
        if (SlotTexture is not null) bits.SetBit(16);
        if (BlockSlotTexture is not null) bits.SetBit(32);
        if (SpecialSlotTexture is not null) bits.SetBit(64);
        if (LocalizationKey is not null) bits.SetBit(128);

        writer.WriteBits(bits);

        if (TextColor is not null)
        {
            TextColor.Serialize(writer);
        }
        else
        {
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }

        writer.WriteBoolean(VisibleQualityLabel);
        writer.WriteBoolean(RenderSpecialSlot);
        writer.WriteBoolean(HideFromSearch);

        var idOffsetSlot = writer.ReserveOffset();
        var itemTooltipTextureOffsetSlot = writer.ReserveOffset();
        var itemTooltipArrowTextureOffsetSlot = writer.ReserveOffset();
        var slotTextureOffsetSlot = writer.ReserveOffset();
        var blockSlotTextureOffsetSlot = writer.ReserveOffset();
        var specialSlotTextureOffsetSlot = writer.ReserveOffset();
        var localizationKeyOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Id is not null) writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(idOffsetSlot, -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        if (ItemTooltipTexture is not null)
            writer.WriteOffsetAt(itemTooltipTextureOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(itemTooltipTextureOffsetSlot, -1);
        if (ItemTooltipTexture is not null) writer.WriteVarUtf8String(ItemTooltipTexture, 4096000);

        if (ItemTooltipArrowTexture is not null)
            writer.WriteOffsetAt(itemTooltipArrowTextureOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(itemTooltipArrowTextureOffsetSlot, -1);
        if (ItemTooltipArrowTexture is not null) writer.WriteVarUtf8String(ItemTooltipArrowTexture, 4096000);

        if (SlotTexture is not null) writer.WriteOffsetAt(slotTextureOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(slotTextureOffsetSlot, -1);
        if (SlotTexture is not null) writer.WriteVarUtf8String(SlotTexture, 4096000);

        if (BlockSlotTexture is not null)
            writer.WriteOffsetAt(blockSlotTextureOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(blockSlotTextureOffsetSlot, -1);
        if (BlockSlotTexture is not null) writer.WriteVarUtf8String(BlockSlotTexture, 4096000);

        if (SpecialSlotTexture is not null)
            writer.WriteOffsetAt(specialSlotTextureOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(specialSlotTextureOffsetSlot, -1);
        if (SpecialSlotTexture is not null) writer.WriteVarUtf8String(SpecialSlotTexture, 4096000);

        if (LocalizationKey is not null)
            writer.WriteOffsetAt(localizationKeyOffsetSlot, writer.Position - varBlockStart);
        else writer.WriteOffsetAt(localizationKeyOffsetSlot, -1);
        if (LocalizationKey is not null) writer.WriteVarUtf8String(LocalizationKey, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            TextColor = reader.ReadObject<Color>();
        }
        else
        {
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }

        VisibleQualityLabel = reader.ReadBoolean();
        RenderSpecialSlot = reader.ReadBoolean();
        HideFromSearch = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(7);

        if (bits.IsSet(2)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(4)) ItemTooltipTexture = reader.ReadVarUtf8StringAt(offsets[1]);
        if (bits.IsSet(8)) ItemTooltipArrowTexture = reader.ReadVarUtf8StringAt(offsets[2]);
        if (bits.IsSet(16)) SlotTexture = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(32)) BlockSlotTexture = reader.ReadVarUtf8StringAt(offsets[4]);
        if (bits.IsSet(64)) SpecialSlotTexture = reader.ReadVarUtf8StringAt(offsets[5]);
        if (bits.IsSet(128)) LocalizationKey = reader.ReadVarUtf8StringAt(offsets[6]);
    }
}