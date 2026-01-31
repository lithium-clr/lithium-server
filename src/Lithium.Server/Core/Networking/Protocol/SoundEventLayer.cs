namespace Lithium.Server.Core.Networking.Protocol;

public sealed record SoundEventLayer : INetworkSerializable
{
    public float Volume { get; init; } = 1f;
    public float StartDelay { get; init; }
    public bool Looping { get; init; }
    public int Probability { get; init; } = 100;
    public float ProbabilityRerollDelay { get; init; } = 1f;
    public SoundEventLayerRandomSettings RandomSettings { get; init; } = new();
    public int RoundRobinHistorySize { get; init; }
    public string[]? Files { get; init; }
    
    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }

    public sealed record SoundEventLayerRandomSettings : INetworkSerializable
    {
        public float MinVolume { get; init; } = 1f;
        public float MaxVolume { get; init; } = 1f;
        public float MinPitch { get; init; } = 1f;
        public float MaxPitch { get; init; } = 1f;
        public float MaxStartOffset { get; init; }

        public void Serialize(PacketWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(PacketReader reader)
        {
            throw new NotImplementedException();
        }
    }
}