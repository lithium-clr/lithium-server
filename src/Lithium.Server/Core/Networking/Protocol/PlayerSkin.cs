using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PlayerSkin : INetworkSerializable
{
    [JsonPropertyName("bodyCharacteristic")]
    public string? BodyCharacteristic { get; set; }

    [JsonPropertyName("underwear")] public string? Underwear { get; set; }
    [JsonPropertyName("face")] public string? Face { get; set; }
    [JsonPropertyName("eyes")] public string? Eyes { get; set; }
    [JsonPropertyName("ears")] public string? Ears { get; set; }
    [JsonPropertyName("mouth")] public string? Mouth { get; set; }
    [JsonPropertyName("facialHair")] public string? FacialHair { get; set; }
    [JsonPropertyName("haircut")] public string? Haircut { get; set; }
    [JsonPropertyName("eyebrows")] public string? Eyebrows { get; set; }
    [JsonPropertyName("pants")] public string? Pants { get; set; }
    [JsonPropertyName("overpants")] public string? Overpants { get; set; }
    [JsonPropertyName("undertop")] public string? Undertop { get; set; }
    [JsonPropertyName("overtop")] public string? Overtop { get; set; }
    [JsonPropertyName("shoes")] public string? Shoes { get; set; }
    [JsonPropertyName("headAccessory")] public string? HeadAccessory { get; set; }
    [JsonPropertyName("faceAccessory")] public string? FaceAccessory { get; set; }
    [JsonPropertyName("earAccessory")] public string? EarAccessory { get; set; }
    [JsonPropertyName("skinFeature")] public string? SkinFeature { get; set; }
    [JsonPropertyName("gloves")] public string? Gloves { get; set; }
    [JsonPropertyName("cape")] public string? Cape { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[3];

        if (BodyCharacteristic is not null) nullBits[0] |= 1;
        if (Underwear is not null) nullBits[0] |= 2;
        if (Face is not null) nullBits[0] |= 4;
        if (Eyes is not null) nullBits[0] |= 8;
        if (Ears is not null) nullBits[0] |= 16;
        if (Mouth is not null) nullBits[0] |= 32;
        if (FacialHair is not null) nullBits[0] |= 64;
        if (Haircut is not null) nullBits[0] |= 128;
        if (Eyebrows is not null) nullBits[1] |= 1;
        if (Pants is not null) nullBits[1] |= 2;
        if (Overpants is not null) nullBits[1] |= 4;
        if (Undertop is not null) nullBits[1] |= 8;
        if (Overtop is not null) nullBits[1] |= 16;
        if (Shoes is not null) nullBits[1] |= 32;
        if (HeadAccessory is not null) nullBits[1] |= 64;
        if (FaceAccessory is not null) nullBits[1] |= 128;
        if (EarAccessory is not null) nullBits[2] |= 1;
        if (SkinFeature is not null) nullBits[2] |= 2;
        if (Gloves is not null) nullBits[2] |= 4;
        if (Cape is not null) nullBits[2] |= 8;

        foreach (var b in nullBits)
            writer.WriteUInt8(b);

        var offsets = new int[20];

        for (var i = 0; i < 20; i++)
            offsets[i] = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        WriteField(writer, offsets[0], BodyCharacteristic, varBlockStart);
        WriteField(writer, offsets[1], Underwear, varBlockStart);
        WriteField(writer, offsets[2], Face, varBlockStart);
        WriteField(writer, offsets[3], Eyes, varBlockStart);
        WriteField(writer, offsets[4], Ears, varBlockStart);
        WriteField(writer, offsets[5], Mouth, varBlockStart);
        WriteField(writer, offsets[6], FacialHair, varBlockStart);
        WriteField(writer, offsets[7], Haircut, varBlockStart);
        WriteField(writer, offsets[8], Eyebrows, varBlockStart);
        WriteField(writer, offsets[9], Pants, varBlockStart);
        WriteField(writer, offsets[10], Overpants, varBlockStart);
        WriteField(writer, offsets[11], Undertop, varBlockStart);
        WriteField(writer, offsets[12], Overtop, varBlockStart);
        WriteField(writer, offsets[13], Shoes, varBlockStart);
        WriteField(writer, offsets[14], HeadAccessory, varBlockStart);
        WriteField(writer, offsets[15], FaceAccessory, varBlockStart);
        WriteField(writer, offsets[16], EarAccessory, varBlockStart);
        WriteField(writer, offsets[17], SkinFeature, varBlockStart);
        WriteField(writer, offsets[18], Gloves, varBlockStart);
        WriteField(writer, offsets[19], Cape, varBlockStart);
    }

    private static void WriteField(PacketWriter writer, int slot, string? value, int varBlockStart)
    {
        if (value is not null)
        {
            writer.WriteOffsetAt(slot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(value, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(slot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var nullBits = new byte[3];

        for (var i = 0; i < 3; i++)
            nullBits[i] = reader.ReadUInt8();

        var offsets = reader.ReadOffsets(20);
        var varStart = reader.GetPosition();
        var maxPos = varStart;

        if ((nullBits[0] & 1) != 0) BodyCharacteristic = ReadAt(offsets[0]);
        if ((nullBits[0] & 2) != 0) Underwear = ReadAt(offsets[1]);
        if ((nullBits[0] & 4) != 0) Face = ReadAt(offsets[2]);
        if ((nullBits[0] & 8) != 0) Eyes = ReadAt(offsets[3]);
        if ((nullBits[0] & 16) != 0) Ears = ReadAt(offsets[4]);
        if ((nullBits[0] & 32) != 0) Mouth = ReadAt(offsets[5]);
        if ((nullBits[0] & 64) != 0) FacialHair = ReadAt(offsets[6]);
        if ((nullBits[0] & 128) != 0) Haircut = ReadAt(offsets[7]);

        if ((nullBits[1] & 1) != 0) Eyebrows = ReadAt(offsets[8]);
        if ((nullBits[1] & 2) != 0) Pants = ReadAt(offsets[9]);
        if ((nullBits[1] & 4) != 0) Overpants = ReadAt(offsets[10]);
        if ((nullBits[1] & 8) != 0) Undertop = ReadAt(offsets[11]);
        if ((nullBits[1] & 16) != 0) Overtop = ReadAt(offsets[12]);
        if ((nullBits[1] & 32) != 0) Shoes = ReadAt(offsets[13]);
        if ((nullBits[1] & 64) != 0) HeadAccessory = ReadAt(offsets[14]);
        if ((nullBits[1] & 128) != 0) FaceAccessory = ReadAt(offsets[15]);

        if ((nullBits[2] & 1) != 0) EarAccessory = ReadAt(offsets[16]);
        if ((nullBits[2] & 2) != 0) SkinFeature = ReadAt(offsets[17]);
        if ((nullBits[2] & 4) != 0) Gloves = ReadAt(offsets[18]);
        if ((nullBits[2] & 8) != 0) Cape = ReadAt(offsets[19]);

        // On place le curseur à la fin des données lues pour ne pas désynchroniser le flux
        reader.SeekTo(maxPos);

        return;
        
        string? ReadAt(int offset)
        {
            if (offset == -1) return null;

            var saved = reader.GetPosition();
            reader.SeekTo(varStart + offset);

            var s = reader.ReadUtf8String();
            
            if (reader.GetPosition() > maxPos) maxPos = reader.GetPosition();
            reader.SeekTo(saved);

            return s;
        }
    }
}