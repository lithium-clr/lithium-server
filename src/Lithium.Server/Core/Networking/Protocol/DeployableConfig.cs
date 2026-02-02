using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<CameraActionType>))]
public enum CameraActionType : byte
{
    ForcePerspective = 0,
    Orbit = 1,
    Transition = 2
}

[JsonConverter(typeof(EnumStringConverter<CameraPerspectiveType>))]
public enum CameraPerspectiveType : byte
{
    First = 0,
    Third = 1
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 2,
    VariableBlockStart = 10,
    MaxSize = 2058
)]
public sealed class DeployableConfig : INetworkSerializable
{
    [JsonPropertyName("model")]            public Model? Model            { get; set; }
    [JsonPropertyName("modelPreview")]     public Model? ModelPreview     { get; set; }
    [JsonPropertyName("allowPlaceOnWalls")] public bool   AllowPlaceOnWalls { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Model is not null)        bits.SetBit(1);
        if (ModelPreview is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteBoolean(AllowPlaceOnWalls);

        var modelOffsetSlot        = writer.ReserveOffset();
        var modelPreviewOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Model is not null)
        {
            writer.WriteOffsetAt(modelOffsetSlot, writer.Position - varBlockStart);
            Model.Serialize(writer);
        }
        else writer.WriteOffsetAt(modelOffsetSlot, -1);

        if (ModelPreview is not null)
        {
            writer.WriteOffsetAt(modelPreviewOffsetSlot, writer.Position - varBlockStart);
            ModelPreview.Serialize(writer);
        }
        else writer.WriteOffsetAt(modelPreviewOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        AllowPlaceOnWalls = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1)) Model = reader.ReadObjectAt<Model>(offsets[0]);
        if (bits.IsSet(2)) ModelPreview = reader.ReadObjectAt<Model>(offsets[1]);
    }

    public int ComputeSize() => 10;
}
