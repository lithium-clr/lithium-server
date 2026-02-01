namespace Lithium.Server.Core.Networking;

public sealed class BitSet
{
    private readonly byte[] _bits;

    public int ByteCount => _bits.Length;

    public BitSet(int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(byteCount, 32);
        _bits = new byte[byteCount];
    }

    public BitSet(ReadOnlySpan<byte> bits)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bits.Length, 32);
        _bits = bits.ToArray();
    }

    public void SetBit(int bit)
    {
        if (_bits.Length == 0) return;
        if (bit < 1 || (bit & (bit - 1)) != 0)
            throw new ArgumentException("Bit must be a power of two (1, 2, 4, 8, etc.).");

        _bits[0] |= (byte)bit;  // Simplifié : toujours dans le premier byte
    }

    public bool IsSet(int bit)
    {
        if (_bits.Length == 0) return false;
        if (bit < 1 || (bit & (bit - 1)) != 0)
            throw new ArgumentException("Bit must be a power of two (1, 2, 4, 8, etc.).");

        return (_bits[0] & (byte)bit) != 0;
    }

    public void CopyTo(Span<byte> destination) => _bits.CopyTo(destination);

    public ReadOnlySpan<byte> AsSpan()
    {
        return _bits;
    }
}