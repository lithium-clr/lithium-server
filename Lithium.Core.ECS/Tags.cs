using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public struct Tags : IEnumerable<Tag>, IEquatable<Tags>
{
    private TagBitset _bitset;
    public int Count { get; private set; }

    public static Tags Empty => default;

    public Tag this[int index] => Get(index);

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Tag Get(int index)
    {
        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException();

        var found = 0;

        for (var blk = 0; blk < TagBitset.ULongsPerBlock; blk++)
        {
            var block = _bitset.Data[blk];

            while (block != 0)
            {
                var bit = BitOperations.TrailingZeroCount(block);

                if (found == index)
                    return new Tag(blk * TagBitset.BitsPerUlong + bit);

                block &= block - 1;
                found++;
            }
        }

        throw new IndexOutOfRangeException();
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tag Get<T1>() where T1 : struct, ITag
    {
        var id = TagTypeId<T1>.Id;
        return _bitset.Has(id) ? new Tag(id) : throw new KeyNotFoundException();
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Tag, Tag) Get<T1, T2>() where T1 : struct, ITag where T2 : struct, ITag
    {
        var id1 = TagTypeId<T1>.Id;
        var id2 = TagTypeId<T2>.Id;
        
        return _bitset.Has(id1) & _bitset.Has(id2) ? (new Tag(id1), new Tag(id2)) : throw new KeyNotFoundException();
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Tag, Tag, Tag) Get<T1, T2, T3>() where T1 : struct, ITag where T2 : struct, ITag where T3 : struct, ITag
    {
        var id1 = TagTypeId<T1>.Id;
        var id2 = TagTypeId<T2>.Id;
        var id3 = TagTypeId<T3>.Id;
        
        return _bitset.Has(id1) & _bitset.Has(id2) & _bitset.Has(id3) ? (new Tag(id1), new Tag(id2), new Tag(id3)) : throw new KeyNotFoundException();
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int id)
    {
        if (_bitset.Has(id)) return;
        _bitset.Add(id);

        Count++;
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;

        if (_bitset.Has(id)) return;
        _bitset.Add(id);

        Count++;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int id)
    {
        if (!_bitset.Has(id)) return;
        _bitset.Remove(id);

        Count--;
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;

        if (!_bitset.Has(id)) return;
        _bitset.Remove(id);

        Count--;
    }
   
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int tagId)
    {
        return _bitset.Has(tagId);
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(in Tags other)
    {
        return _bitset.HasAll(other._bitset);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, ITag
    {
        return _bitset.Has(TagTypeId<T>.Id);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T1, T2>()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        return Has<T1>() & Has<T2>();
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T1, T2, T3>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
    {
        return Has<T1>() & Has<T2>() & Has<T3>();
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAll(in Tags other)
    {
        return _bitset.HasAll(other._bitset);
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAny(in Tags other)
    {
        return _bitset.HasAny(other._bitset);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsSpan(Span<int> destination)
    {
        var count = 0;

        for (var i = 0; i < TagBitset.ULongsPerBlock; i++)
        {
            var block = _bitset.Data[i];

            while (block != 0)
            {
                var bit = BitOperations.TrailingZeroCount(block);

                if (count >= destination.Length)
                    throw new ArgumentException("Destination span is too small", nameof(destination));

                destination[count++] = i * TagBitset.BitsPerUlong + bit;
                block &= block - 1;
            }
        }

        return count;
    }

    public Enumerator GetEnumerator() => new(ref _bitset, Count);
    IEnumerator<Tag> IEnumerable<Tag>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<Tag>
    {
        private readonly TagBitset _bitset;
        private readonly int _count;
        private int _index;
        private int _current;

        internal Enumerator(ref TagBitset bitset, int count)
        {
            _bitset = bitset;
            _count = count;
            _index = -1;
            _current = -1;
        }

        public Tag Current => new(_current);
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            if (_index >= _count) return false;

            var found = 0;

            for (var blk = 0; blk < TagBitset.ULongsPerBlock; blk++)
            {
                var block = _bitset.Data[blk];

                while (block != 0)
                {
                    var bit = BitOperations.TrailingZeroCount(block);

                    if (found == _index)
                    {
                        _current = blk * TagBitset.BitsPerUlong + bit;
                        return true;
                    }

                    block &= block - 1;
                    found++;
                }
            }

            return false;
        }

        public void Reset() => _index = -1;

        public void Dispose()
        {
        }
    }

    public bool Equals(Tags other) => _bitset.Equals(other._bitset) && Count == other.Count;
    public override bool Equals(object? obj) => obj is Tags other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_bitset, Count);

    public static bool operator ==(Tags left, Tags right) => left.Equals(right);
    public static bool operator !=(Tags left, Tags right) => !(left == right);
}