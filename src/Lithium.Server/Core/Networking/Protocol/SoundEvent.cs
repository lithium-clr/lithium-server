using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class SoundEvent : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volume")]
    public float Volume { get; set; }

    [JsonPropertyName("pitch")]
    public float Pitch { get; set; }

    [JsonPropertyName("musicDuckingVolume")]
    public float MusicDuckingVolume { get; set; }

    [JsonPropertyName("ambientDuckingVolume")]
    public float AmbientDuckingVolume { get; set; }

    [JsonPropertyName("maxInstance")]
    public int MaxInstance { get; set; }

    [JsonPropertyName("preventSoundInterruption")]
    public bool PreventSoundInterruption { get; set; }

    [JsonPropertyName("startAttenuationDistance")]
    public float StartAttenuationDistance { get; set; }

    [JsonPropertyName("maxDistance")]
    public float MaxDistance { get; set; }

    [JsonPropertyName("layers")]
    public SoundEventLayer[]? Layers { get; set; }

    [JsonPropertyName("audioCategory")]
    public int AudioCategory { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (Layers is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        writer.WriteFloat32(Volume);
        writer.WriteFloat32(Pitch);
        writer.WriteFloat32(MusicDuckingVolume);
        writer.WriteFloat32(AmbientDuckingVolume);
        writer.WriteInt32(MaxInstance);
        writer.WriteBoolean(PreventSoundInterruption);
        writer.WriteFloat32(StartAttenuationDistance);
        writer.WriteFloat32(MaxDistance);
        writer.WriteInt32(AudioCategory);

        var idOffsetSlot = writer.ReserveOffset();
        var layersOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(layersOffsetSlot, Layers is not null ? writer.Position - varBlockStart : -1);
        if (Layers is not null)
        {
            writer.WriteVarInt(Layers.Length);
            foreach (var layer in Layers)
            {
                layer.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        Volume = reader.ReadFloat32();
        Pitch = reader.ReadFloat32();
        MusicDuckingVolume = reader.ReadFloat32();
        AmbientDuckingVolume = reader.ReadFloat32();
        MaxInstance = reader.ReadInt32();
        PreventSoundInterruption = reader.ReadBoolean();
        StartAttenuationDistance = reader.ReadFloat32();
        MaxDistance = reader.ReadFloat32();
        AudioCategory = reader.ReadInt32();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Layers = new SoundEventLayer[count];
            for (var i = 0; i < count; i++)
            {
                Layers[i] = reader.ReadObject<SoundEventLayer>();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
