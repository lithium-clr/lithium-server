using System.Numerics;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public record struct Vector3Float : INetworkSerializable
{
    private Vector3 _value;

    public Vector3Float()
    {
        _value = Vector3.Zero;
    }

    public Vector3Float(Vector3 value)
    {
        _value = value;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(_value.X);
        writer.WriteFloat32(_value.Y);
        writer.WriteFloat32(_value.Z);
    }

    public void Deserialize(PacketReader reader)
    {
        _value = new Vector3(
            reader.ReadFloat32(),
            reader.ReadFloat32(),
            reader.ReadFloat32()
        );
    }

    public static implicit operator Vector3(Vector3Float obj) => obj._value;
    public static implicit operator Vector3Float(Vector3 obj) => new(obj);
}