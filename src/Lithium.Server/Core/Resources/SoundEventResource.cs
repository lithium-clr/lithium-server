using System.ComponentModel.DataAnnotations;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core.Resources;

public sealed record SoundEventResource : AssetResource
{
    [Range(-100f, 10f)]
    public float Volume
    {
        get => AudioUtil.LinearGainToDecibels(field);
        set => field = AudioUtil.DecibelsToLinearGain(value);
    } = 1f;

    [Range(-12f, 12f)]
    public float Pitch
    {
        get => AudioUtil.LinearGainToDecibels(field);
        set => field = AudioUtil.DecibelsToLinearGain(value);
    } = 1f;

    [Range(-100f, 0f)]
    public float MusicDuckingVolume
    {
        get => AudioUtil.LinearGainToDecibels(field);
        set => field = AudioUtil.DecibelsToLinearGain(value);
    } = 1f;

    [Range(-100f, 0f)]
    public float AmbientDuckingVolume
    {
        get => AudioUtil.LinearGainToDecibels(field);
        set => field = AudioUtil.DecibelsToLinearGain(value);
    } = 1f;

    public float StartAttenuationDistance { get; set; } = 2f;
    public float MaxDistance { get; set; } = 16f;
    [Range(1, 100)] public int MaxInstance { get; set; } = 50;
    public bool PreventSoundInterruption { get; set; }
    public SoundEventLayer[] Layers { get; set; } = [];
    public string? AudioCategory { get; set; } = string.Empty;

    public override PacketObject ToPacket() => new SoundEvent
    {
        Id = FileName,
        Volume = Volume,
        Pitch = Pitch,
        MusicDuckingVolume = MusicDuckingVolume,
        AmbientDuckingVolume = AmbientDuckingVolume,
        StartAttenuationDistance = StartAttenuationDistance,
        MaxDistance = MaxDistance,
        MaxInstance = MaxInstance,
        PreventSoundInterruption = PreventSoundInterruption,
        Layers = Layers,
        AudioCategory = AudioCategory
    };
}