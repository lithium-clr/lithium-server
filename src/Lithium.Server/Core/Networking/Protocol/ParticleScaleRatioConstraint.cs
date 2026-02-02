using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticleScaleRatioConstraint : byte
{
    OneToOne = 0,
    Preserved = 1,
    None = 2
}