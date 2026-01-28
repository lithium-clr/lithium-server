using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record FloatRange : PacketObject
{
    [PacketProperty(FixedIndex = 0)]
    public float InclusiveMin { get; init; }

    [PacketProperty(FixedIndex = 1)]
    public float InclusiveMax { get; init; }

    public FloatRange()
    {
    }

    private FloatRange(float inclusiveMin, float inclusiveMax)
    {
        InclusiveMin = inclusiveMin;
        InclusiveMax = inclusiveMax;
    }

    public static FloatRange Default => new(.5f, 1.5f);
}