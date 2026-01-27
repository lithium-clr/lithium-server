namespace Lithium.Server.Core.Networking.Protocol;

public sealed class BitSet
{
    private const int MinBitsetLength = 0;
    private const int MaxBitsetLength = 32;

    private readonly byte[] _bits;

    public int ByteCount => _bits.Length;

    public BitSet(int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(byteCount, MinBitsetLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(byteCount, MaxBitsetLength);

        _bits = byteCount is 0 ? [] : new byte[byteCount];
    }

    public BitSet(ReadOnlySpan<byte> bits)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bits.Length, MaxBitsetLength);

        _bits = bits.ToArray();
    }

    public void SetBit(int bitIndex)
    {
        if (_bits.Length is 0) return;

        var index = bitIndex / 8;
        var offset = bitIndex % 8;

        _bits[index] |= (byte)(1 << offset);
    }

    public bool IsSet(int bitIndex)
    {
        if (_bits.Length is 0) return false;

        var index = bitIndex / 8;
        var offset = bitIndex % 8;

        return (_bits[index] & (byte)(1 << offset)) is not 0;
    }

    public void CopyTo(Span<byte> destination)
    {
        _bits.CopyTo(destination);
    }

    public override string ToString()
    {
        return $"BitSet({string.Join(' ', _bits.Select(static b => Convert.ToString(b, 2).PadLeft(8, '0')))})";
    }
}