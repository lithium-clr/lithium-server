namespace Lithium.Server.Core.Networking.Protocol;

public struct RangeFloat : INetworkSerializable
{
    public float Min { get; set; }
    public float Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Min);
        writer.WriteFloat32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadFloat32();
        Max = reader.ReadFloat32();
    }
}