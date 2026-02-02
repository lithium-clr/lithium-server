using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UVMotionCurveType : byte
{
    Constant = 0,
    IncreaseLinear = 1,
    IncreaseQuartIn = 2,
    IncreaseQuartInOut = 3,
    IncreaseQuartOut = 4,
    DecreaseLinear = 5,
    DecreaseQuartIn = 6,
    DecreaseQuartInOut = 7,
    DecreaseQuartOut = 8
}