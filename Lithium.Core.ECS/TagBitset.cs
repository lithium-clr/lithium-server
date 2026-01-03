using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lithium.Core.ECS;

public unsafe struct TagBitset : IEquatable<TagBitset>
{
    public const int BitsPerUlong = 64;
    public const int BitsPerBlock = 256; // Must be multiple of BitsPerUlong
    public const int ULongsPerBlock = BitsPerBlock / BitsPerUlong;

    private fixed ulong _data[ULongsPerBlock];

    public readonly Span<ulong> Data
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            fixed (ulong* ptr = _data)
                return new Span<ulong>(ptr, ULongsPerBlock);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int tagId)
    {
        var idx = tagId >> 6;
        var bit = tagId & 63;

        _data[idx] |= 1UL << bit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int tagId)
    {
        var idx = tagId >> 6;
        var bit = tagId & 63;

        _data[idx] &= ~(1UL << bit);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int tagId)
    {
        var idx = tagId >> 6;
        var bit = tagId & 63;

        return (_data[idx] & (1UL << bit)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAll(in TagBitset other)
    {
        fixed (ulong* a = _data)
        fixed (ulong* b = other._data)
        {
            if (Avx2.IsSupported)
            {
                var va = Avx.LoadVector256(a);
                var vb = Avx.LoadVector256(b);

                return Avx.TestC(va, vb);
            }

            for (var i = 0; i < ULongsPerBlock; i++)
                if ((a[i] & b[i]) != b[i])
                    return false;

            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAny(in TagBitset other)
    {
        fixed (ulong* a = _data)
        fixed (ulong* b = other._data)
        {
            if (Avx2.IsSupported)
            {
                var va = Avx.LoadVector256(a);
                var vb = Avx.LoadVector256(b);
                var and = Avx2.And(va, vb);

                return !Avx.TestZ(and, and);
            }

            for (var i = 0; i < ULongsPerBlock; i++)
                if ((a[i] & b[i]) != 0)
                    return true;

            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        for (var i = 0; i < ULongsPerBlock; i++)
            _data[i] = 0;
    }

    public void ForEachTag(Action<int> action)
    {
        for (var i = 0; i < ULongsPerBlock; i++)
        {
            var block = _data[i];

            while (block != 0)
            {
                var bit = BitOperations.TrailingZeroCount(block);
                action(i * BitsPerUlong + bit);
                block &= block - 1;
            }
        }
    }

    public bool Equals(TagBitset other)
    {
        for (var i = 0; i < ULongsPerBlock; i++)
            if (_data[i] != other._data[i])
                return false;

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is TagBitset other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;

        for (var i = 0; i < ULongsPerBlock; i++)
            hash = hash * 31 + _data[i].GetHashCode();

        return hash;
    }

    public static bool operator ==(TagBitset left, TagBitset right) => left.Equals(right);
    public static bool operator !=(TagBitset left, TagBitset right) => !left.Equals(right);
}