using System.Reflection;

namespace Hytale.Core.Networking;

public sealed class PacketRegistry
{
    private readonly Dictionary<ushort, Type> _types = new();

    public void RegisterType<T>() where T : unmanaged, IPacket
    {
        var id = GetPacketId<T>();
        _types[id] = typeof(T);
    }

    public ushort GetPacketId<T>() where T : unmanaged, IPacket
    {
        var attr = typeof(T).GetCustomAttribute<PacketIdAttribute>();
        return attr?.Id ?? throw new InvalidOperationException("Missing PacketIdAttribute");
    }

    public ushort GetPacketId(Type packetType)
    {
        var attr = packetType.GetCustomAttribute<PacketIdAttribute>();
        return attr?.Id ?? throw new InvalidOperationException("Missing PacketIdAttribute");
    }

    public Type? GetPacketType(ushort typeId)
    {
        return _types.FirstOrDefault(x => x.Key == typeId).Value;
    }
}