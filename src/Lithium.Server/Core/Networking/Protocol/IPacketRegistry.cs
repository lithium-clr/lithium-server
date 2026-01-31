using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

public interface IPacketRegistry
{
    void Register(PacketInfo packetInfo);
    void Register(Type type);
    void RegisterAllFromAssembly(Assembly assembly);
    bool TryGetPacketInfoById(int packetId, [NotNullWhen(true)] out PacketInfo? packetInfo);
    bool TryGetPacketInfoByType(Type packetType, [NotNullWhen(true)] out PacketInfo? packetInfo);
    bool TryGetPacketInstanceById(int packetId, [NotNullWhen(true)] out INetworkSerializable? packet);
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
        var packetAttribute = type.GetCustomAttribute<PacketAttribute>();
        
        if (packetAttribute is null)
        {
            // Or log a warning
            return;
        }

        // By convention, the fixed block size is a public const field on the packet class.
        // var fixedBlockSizeField = type.GetField("FixedBlockTotalSize", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        // var fixedBlockSize = fixedBlockSizeField?.GetValue(null) as int? ?? 0;

        var packetInfo = new PacketInfo(
            packetAttribute.Id,
            type.Name,
            type,
            packetAttribute.IsCompressed,
            packetAttribute.NullableBitFieldSize,
            packetAttribute.FixedBlockSize,
            packetAttribute.VariableFieldCount,
            packetAttribute.VariableBlockStart,
            packetAttribute.MaxSize
        );
        
        Register(packetInfo);
    }

    public void RegisterAllFromAssembly(Assembly assembly)
    {
        var packetTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(INetworkSerializable).IsAssignableFrom(t));

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

    public bool TryGetPacketInstanceById(int packetId, [NotNullWhen(true)] out INetworkSerializable? packet)
    {
        if (TryGetPacketInfoById(packetId, out var packetInfo))
        {
            packet = (INetworkSerializable)Activator.CreateInstance(packetInfo.PacketType)!;
            return true;
        }

        packet = null;
        return false;
    }
}