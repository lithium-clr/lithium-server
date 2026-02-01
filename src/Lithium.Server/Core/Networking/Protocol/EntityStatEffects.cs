using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class EntityStatEffects : INetworkSerializable
{
    [JsonPropertyName("triggerAtZero")]
    public bool TriggerAtZero { get; set; }

    [JsonPropertyName("soundEventIndex")]
    public int SoundEventIndex { get; set; }

    [JsonPropertyName("particles")]
    public ModelParticle[]? Particles { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Particles is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteBoolean(TriggerAtZero);
        writer.WriteInt32(SoundEventIndex);

        if (Particles is not null)
        {
            writer.WriteVarInt(Particles.Length);
            foreach (var particle in Particles)
            {
                particle.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        TriggerAtZero = reader.ReadBoolean();
        SoundEventIndex = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Particles = new ModelParticle[count];
            for (var i = 0; i < count; i++)
            {
                Particles[i] = reader.ReadObject<ModelParticle>();
            }
        }
    }
}
