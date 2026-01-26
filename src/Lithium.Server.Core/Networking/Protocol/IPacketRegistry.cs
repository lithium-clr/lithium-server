using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Lithium.Server.Core.Networking.Protocol;

public interface IPacketRegistry
{
    void Register(PacketInfo packetInfo);
    void Register(Type type);
    void RegisterAllFromAssembly(Assembly assembly);
    bool TryGetPacketInfoById(int packetId, [NotNullWhen(true)] out PacketInfo? packetInfo);
    bool TryGetPacketInfoByType(Type packetType, [NotNullWhen(true)] out PacketInfo? packetInfo);
    bool TryGetPacketInstanceById(int packetId, [NotNullWhen(true)] out Packet? packet);
}

public sealed class PacketRegistry : IPacketRegistry
{
    private readonly Dictionary<int, PacketInfo> _packetInfos = new();
    private readonly Dictionary<Type, PacketInfo> _packetInfosByType = new();

    public void Register(PacketInfo packetInfo)
    {
        _packetInfos[packetInfo.PacketId] = packetInfo;
        _packetInfosByType[packetInfo.PacketType] = packetInfo;
    }

    public void Register(Type type)
    {
        var metadata = GenericPacketHelpers.GetMetadata(type);
        if (metadata.PacketInfo is not null)
        {
            Register(metadata.PacketInfo);
        }
    }

    public void RegisterAllFromAssembly(Assembly assembly)
    {
        var packetTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Packet).IsAssignableFrom(t));

        foreach (var type in packetTypes)
        {
            Register(type);
        }
    }

    public bool TryGetPacketInfoById(int packetId, [NotNullWhen(true)] out PacketInfo? packetInfo)
    {
        return _packetInfos.TryGetValue(packetId, out packetInfo);
    }

    public bool TryGetPacketInfoByType(Type packetType, [NotNullWhen(true)] out PacketInfo? packetInfo)
    {
        return _packetInfosByType.TryGetValue(packetType, out packetInfo);
    }

    public bool TryGetPacketInstanceById(int packetId, [NotNullWhen(true)] out Packet? packet)
    {
        if (TryGetPacketInfoById(packetId, out var packetInfo))
        {
            packet = (Packet)Activator.CreateInstance(packetInfo.PacketType)!;
            return true;
        }

        packet = null;
        return false;
    }
}