using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolBlockArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("allowPattern")]
    public bool AllowPattern { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (DefaultValue is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteBoolean(AllowPattern);

        if (DefaultValue is not null)
        {
            writer.WriteVarUtf8String(DefaultValue, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        AllowPattern = reader.ReadBoolean();

        if (bits.IsSet(1))
        {
            DefaultValue = reader.ReadUtf8String();
        }
    }
}
