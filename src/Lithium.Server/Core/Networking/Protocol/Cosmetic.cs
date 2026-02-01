using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<Cosmetic>))]
public enum Cosmetic : byte
{
    Haircut = 0,
    FacialHair = 1,
    Undertop = 2,
    Overtop = 3,
    Pants = 4,
    Overpants = 5,
    Shoes = 6,
    Gloves = 7,
    Cape = 8,
    HeadAccessory = 9,
    FaceAccessory = 10,
    EarAccessory = 11,
    Ear = 12
}
