using System.Buffers.Binary;

namespace Lithium.Server.Core.Protocol.Packets;

public readonly struct PlayerSkin(
    string? bodyCharacteristic,
    string? underwear,
    string? face,
    string? eyes,
    string? ears,
    string? mouth,
    string? facialHair,
    string? haircut,
    string? eyebrows,
    string? pants,
    string? overpants,
    string? undertop,
    string? overtop,
    string? shoes,
    string? headAccessory,
    string? faceAccessory,
    string? earAccessory,
    string? skinFeature,
    string? gloves,
    string? cape
)
{
    private const int VariableBlockStart = 83;

    public readonly string? BodyCharacteristic = bodyCharacteristic;
    public readonly string? Underwear = underwear;
    public readonly string? Face = face;
    public readonly string? Eyes = eyes;
    public readonly string? Ears = ears;
    public readonly string? Mouth = mouth;
    public readonly string? FacialHair = facialHair;
    public readonly string? Haircut = haircut;
    public readonly string? Eyebrows = eyebrows;
    public readonly string? Pants = pants;
    public readonly string? OverPants = overpants;
    public readonly string? UnderTop = undertop;
    public readonly string? Overtop = overtop;
    public readonly string? Shoes = shoes;
    public readonly string? HeadAccessory = headAccessory;
    public readonly string? FaceAccessory = faceAccessory;
    public readonly string? EarAccessory = earAccessory;
    public readonly string? SkinFeature = skinFeature;
    public readonly string? Gloves = gloves;
    public readonly string? Cape = cape;

    public static PlayerSkin Deserialize(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadBytes(3);
        var offsets = new int[20];

        for (var i = 0; i < 20; i++)
            offsets[i] = reader.ReadInt32();

        var varBlock = buffer[VariableBlockStart..];

        // Byte 0
        var bodyCharacteristic = (nullBits[0] & 1) is not 0 ? ReadString(varBlock, offsets[0]) : null;
        var underwear = (nullBits[0] & 2) is not 0 ? ReadString(varBlock, offsets[1]) : null;
        var face = (nullBits[0] & 4) is not 0 ? ReadString(varBlock, offsets[2]) : null;
        var eyes = (nullBits[0] & 8) is not 0 ? ReadString(varBlock, offsets[3]) : null;
        var ears = (nullBits[0] & 16) is not 0 ? ReadString(varBlock, offsets[4]) : null;
        var mouth = (nullBits[0] & 32) is not 0 ? ReadString(varBlock, offsets[5]) : null;
        var facialHair = (nullBits[0] & 64) is not 0 ? ReadString(varBlock, offsets[6]) : null;
        var haircut = (nullBits[0] & 128) is not 0 ? ReadString(varBlock, offsets[7]) : null;

        // Byte 1
        var eyebrows = (nullBits[1] & 1) is not 0 ? ReadString(varBlock, offsets[8]) : null;
        var pants = (nullBits[1] & 2) is not 0 ? ReadString(varBlock, offsets[9]) : null;
        var overPants = (nullBits[1] & 4) is not 0 ? ReadString(varBlock, offsets[10]) : null;
        var underTop = (nullBits[1] & 8) is not 0 ? ReadString(varBlock, offsets[11]) : null;
        var overtop = (nullBits[1] & 16) is not 0 ? ReadString(varBlock, offsets[12]) : null;
        var shoes = (nullBits[1] & 32) is not 0 ? ReadString(varBlock, offsets[13]) : null;
        var headAccessory = (nullBits[1] & 64) is not 0 ? ReadString(varBlock, offsets[14]) : null;
        var faceAccessory = (nullBits[1] & 128) is not 0 ? ReadString(varBlock, offsets[15]) : null;

        // Byte 2
        var earAccessory = (nullBits[2] & 1) is not 0 ? ReadString(varBlock, offsets[16]) : null;
        var skinFeature = (nullBits[2] & 2) is not 0 ? ReadString(varBlock, offsets[17]) : null;
        var gloves = (nullBits[2] & 4) is not 0 ? ReadString(varBlock, offsets[18]) : null;
        var cape = (nullBits[2] & 8) is not 0 ? ReadString(varBlock, offsets[19]) : null;

        // Calculate total bytes read
        var maxEnd = VariableBlockStart;

        CheckMaxEnd(nullBits[0] & 1, offsets[0], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 2, offsets[1], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 4, offsets[2], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 8, offsets[3], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 16, offsets[4], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 32, offsets[5], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 64, offsets[6], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[0] & 128, offsets[7], varBlock, ref maxEnd);

        CheckMaxEnd(nullBits[1] & 1, offsets[8], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 2, offsets[9], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 4, offsets[10], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 8, offsets[11], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 16, offsets[12], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 32, offsets[13], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 64, offsets[14], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[1] & 128, offsets[15], varBlock, ref maxEnd);

        CheckMaxEnd(nullBits[2] & 1, offsets[16], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[2] & 2, offsets[17], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[2] & 4, offsets[18], varBlock, ref maxEnd);
        CheckMaxEnd(nullBits[2] & 8, offsets[19], varBlock, ref maxEnd);

        bytesRead = maxEnd;

        return new PlayerSkin(
            bodyCharacteristic, underwear, face, eyes, ears, mouth, facialHair, haircut,
            eyebrows, pants, overPants, underTop, overtop, shoes, headAccessory, faceAccessory,
            earAccessory, skinFeature, gloves, cape
        );
    }

    private static string ReadString(ReadOnlySpan<byte> varBlock, int offset)
    {
        return PacketSerializer.ReadVarString(varBlock[offset..], out _);
    }

    private static void CheckMaxEnd(int flag, int offset, ReadOnlySpan<byte> varBlock, ref int maxEnd)
    {
        if (flag is 0) return;

        var length = PacketSerializer.ReadVarInt(varBlock[offset..], out var varIntLen);

        var endPos = VariableBlockStart + offset + varIntLen + length;
        if (endPos > maxEnd) maxEnd = endPos;
    }

    public void Serialize(Stream stream)
    {
        Span<byte> nullBits = stackalloc byte[3];

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
        if (OverPants is not null) nullBits[1] |= 4;
        if (UnderTop is not null) nullBits[1] |= 8;
        if (Overtop is not null) nullBits[1] |= 16;
        if (Shoes is not null) nullBits[1] |= 32;
        if (HeadAccessory is not null) nullBits[1] |= 64;
        if (FaceAccessory is not null) nullBits[1] |= 128;

        if (EarAccessory is not null) nullBits[2] |= 1;
        if (SkinFeature is not null) nullBits[2] |= 2;
        if (Gloves is not null) nullBits[2] |= 4;
        if (Cape is not null) nullBits[2] |= 8;

        stream.Write(nullBits);

        using var varBlockStream = new MemoryStream();
        var offsets = new int[20];

        WriteStr(BodyCharacteristic, 0);
        WriteStr(Underwear, 1);
        WriteStr(Face, 2);
        WriteStr(Eyes, 3);
        WriteStr(Ears, 4);
        WriteStr(Mouth, 5);
        WriteStr(FacialHair, 6);
        WriteStr(Haircut, 7);
        WriteStr(Eyebrows, 8);
        WriteStr(Pants, 9);
        WriteStr(OverPants, 10);
        WriteStr(UnderTop, 11);
        WriteStr(Overtop, 12);
        WriteStr(Shoes, 13);
        WriteStr(HeadAccessory, 14);
        WriteStr(FaceAccessory, 15);
        WriteStr(EarAccessory, 16);
        WriteStr(SkinFeature, 17);
        WriteStr(Gloves, 18);
        WriteStr(Cape, 19);

        Span<byte> offsetBuffer = stackalloc byte[4];

        foreach (var offset in offsets)
        {
            BinaryPrimitives.WriteInt32LittleEndian(offsetBuffer, offset);
            stream.Write(offsetBuffer);
        }

        varBlockStream.Position = 0;
        varBlockStream.CopyTo(stream);

        return;

        void WriteStr(string? value, int index)
        {
            if (value is null)
            {
                offsets[index] = -1;
                return;
            }

            offsets[index] = (int)varBlockStream.Position;
            PacketSerializer.WriteVarString(varBlockStream, value);
        }
    }
}