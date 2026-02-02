using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BenchType>))]
public enum BenchType : byte
{
    Crafting = 0,
    Processing = 1,
    DiagramCrafting = 2,
    StructuralCrafting = 3
}