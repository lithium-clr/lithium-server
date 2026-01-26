namespace Lithium.Server.Core.Networking.Protocol;

public abstract class Packet<T> where T : Packet<T>
{
    public virtual void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public virtual T Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}