namespace Lithium.Server.Core.Networking.Protocol;

public abstract record PacketObject<T> where T : PacketObject<T>
{
    public virtual void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public virtual T Deserialize(PacketReader reader, int offset)
    {
        throw new NotImplementedException();
    }
}