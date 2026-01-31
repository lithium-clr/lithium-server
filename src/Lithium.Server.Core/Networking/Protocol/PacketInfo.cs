namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Stores metadata about a packet type for registration and routing.
/// </summary>
/// <param name="PacketId">The unique identifier of the packet.</param>
/// <param name="PacketName">The name of the packet type.</param>
/// <param name="PacketType">The actual <see cref="Type"/> of the packet.</param>
/// <param name="IsCompressed">Indicates if the packet's payload should be compressed.</param>
/// <param name="MaxSize">The maximum allowed size of the packet's payload (uncompressed if applicable).</param>
/// <param name="FixedBlockSize">The size of the fixed block for this packet, used by PacketReader to determine variable block start.</param>
public sealed record PacketInfo(
    int PacketId,
    string PacketName,
    Type PacketType,
    bool IsCompressed,
    int NullableBitFieldSize,
    int FixedBlockSize,
    int VariableFieldCount,
    int VariableBlockStart,
    int MaxSize
);
