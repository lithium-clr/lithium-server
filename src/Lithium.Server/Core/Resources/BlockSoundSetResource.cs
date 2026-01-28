using Lithium.Server.Core.Networking;

namespace Lithium.Server.Core.Resources;

public sealed record BlockSoundSetResource : AssetResource
{
    public string? Id { get; init; }
    public Dictionary<BlockSoundEvent, int>? SoundEventIndices { get; init; }
    public FloatRange? MoveInRepeatRange { get; init; } = FloatRange.Default;
}