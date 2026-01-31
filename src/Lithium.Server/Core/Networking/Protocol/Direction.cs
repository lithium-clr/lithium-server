namespace Lithium.Server.Core.Networking.Protocol;

public record struct Direction : INetworkSerializable
{
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public float Roll { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Yaw);
        writer.WriteFloat32(Pitch);
        writer.WriteFloat32(Roll);
    }

    public void Deserialize(PacketReader reader)
    {
        Yaw = reader.ReadFloat32();
        Pitch = reader.ReadFloat32();
        Roll = reader.ReadFloat32();
    }
}