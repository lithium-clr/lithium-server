using System.Collections;
using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public struct Tags : IEnumerable<Tag>
{
    private int[] _tags;
    public int Count { get; private set; }

    public static Tags Empty => default;

    public Tags()
    {
        _tags = new int[4];
        Count = 0;
    }

    public Tags(ReadOnlySpan<int> tags)
    {
        _tags = new int[Math.Max(4, tags.Length)];
        Count = 0;

        foreach (var t in tags)
            Add(t);
    }

    public int this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _tags[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int tagId)
    {
        for (var i = 0; i < Count; i++)
            if (_tags[i] == tagId)
                return;

        if (Count == _tags.Length)
            Grow();

        _tags[Count++] = tagId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>() where T : struct, ITag
        => Add(TagTypeId<T>.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;

        for (var i = 0; i < Count; i++)
        {
            if (_tags[i] != id) continue;
            _tags[i] = _tags[--Count];

            return;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        Array.Resize(ref _tags, _tags.Length * 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int id)
    {
        for (var i = 0; i < Count; i++)
            if (_tags[i] == id)
                return true;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, ITag
        => Has(TagTypeId<T>.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T1, T2>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        => Has<T1>() & Has<T2>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T1, T2, T3>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        => Has<T1>() & Has<T2>() & Has<T3>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<int> AsSpan()
        => _tags.AsSpan(0, Count);

    public Enumerator GetEnumerator()
        => new(_tags, Count);

    IEnumerator<Tag> IEnumerable<Tag>.GetEnumerator()
        => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator : IEnumerator<Tag>
    {
        private readonly int[] _tags;
        private readonly int _count;
        private int _index;

        internal Enumerator(int[] tags, int count)
        {
            _tags = tags;
            _count = count;
            _index = -1;
        }

        public Tag Current => new(_tags[_index]);
        object IEnumerator.Current => Current;

        public bool MoveNext() => ++_index < _count;
        public void Reset() => _index = -1;

        public void Dispose()
        {
        }
    }
}