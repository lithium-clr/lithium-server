using System.ComponentModel.DataAnnotations;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core.Resources;

public sealed record AudioCategoryResource : AssetResource
{
    [Range(-100f, 10f)]
    public float Volume
    {
        get => AudioUtil.LinearGainToDecibels(field);
        set => field = AudioUtil.DecibelsToLinearGain(value);
    } = 1f;

    public override PacketObject ToPacket() => new AudioCategory
    {
        Id = FileName,
        Volume = Volume
    };
}