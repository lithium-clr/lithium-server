namespace Lithium.Server.Core.Networking.Protocol;

public sealed record SoundEvent : INetworkSerializable
{
    public float Volume { get; init; }
    public float Pitch { get; init; }
    public float MusicDuckingVolume { get; init; }
    public float AmbientDuckingVolume { get; init; }
    public int MaxInstance { get; init; }
    public bool PreventSoundInterruption { get; init; }
    public float StartAttenuationDistance { get; init; }
    public float MaxDistance { get; init; }
    public int AudioCategory { get; init; }
    public string? Id { get; init; }
    public SoundEventLayer[]? Layers { get; init; }

    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}