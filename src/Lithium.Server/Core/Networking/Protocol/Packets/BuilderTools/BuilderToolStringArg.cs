using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolStringArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (DefaultValue is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        if (DefaultValue is not null)
        {
            writer.WriteVarUtf8String(DefaultValue, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1))
        {
            DefaultValue = reader.ReadUtf8String();
        }
    }
}
