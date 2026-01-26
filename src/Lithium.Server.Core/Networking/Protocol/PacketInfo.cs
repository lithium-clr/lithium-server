namespace Lithium.Server.Core.Networking.Protocol;

public sealed record PacketInfo(
    int PacketId,
    string PacketName,
    Type PacketType,
    bool IsCompressed,
    int NullableBitFieldSize,
    int VariableFieldCount,
    int VariableBlockStart,
    int MaxSize,
    bool UseOffsets
);
