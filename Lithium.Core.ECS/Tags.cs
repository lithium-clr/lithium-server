using System.Collections;
using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public struct Tags : IEnumerable<Tag>
{
    private int[] _tags = new int[4];
    private int _count = 0;

    public static Tags Empty => [];

    public Tags()
    {
    }

    public Tags(ReadOnlySpan<int> tags)
    {
        foreach (var tag in tags)
            Add(tag);
    }

    public Tag this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();

            return _tags[index];
        }
        set => _tags[index] = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Tag tag)
    {
        if (Contains(tag.Id)) return;

        if (_count == _tags.Length)
            Array.Resize(ref _tags, _tags.Length * 2);

        _tags[_count++] = tag.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>()
        where T : struct, ITag
    {
        Add(TagTypeId<T>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove<T>()
        where T : struct, ITag
    {
        if (!Contains(TagTypeId<T>.Id)) return;

        for (var i = 0; i < _count; i++)
            if (_tags[i] == TagTypeId<T>.Id)
                _tags[i] = _tags[_count--];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Tag Get<T>()
        where T : struct, ITag
    {
        return _tags[TagTypeId<T>.Id];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Tag> Get<T1, T2>()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        return new[] { Get<T1>(), Get<T2>() };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Tag> Get<T1, T2, T3>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
    {
        return new[] { Get<T1>(), Get<T2>(), Get<T3>() };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Tag> Get<T1, T2, T3, T4>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        where T4 : struct, ITag
    {
        return new[] { Get<T1>(), Get<T2>(), Get<T3>(), Get<T4>() };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Tag> Get<T1, T2, T3, T4, T5>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        where T4 : struct, ITag
        where T5 : struct, ITag
    {
        return new[] { Get<T1>(), Get<T2>(), Get<T3>(), Get<T4>(), Get<T5>() };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool Contains(int tagId)
    {
        for (var i = 0; i < _count; i++)
            if (_tags[i] == tagId)
                return true;

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(Tags tags)
    {
        foreach (var tag in tags)
            if (!Contains(tag))
                return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T1>()
        where T1 : struct, ITag
    {
        return Contains(TagTypeId<T1>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T1, T2>()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        return Contains(TagTypeId<T1>.Id)
               && Contains(TagTypeId<T2>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T1, T2, T3>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
    {
        return Contains(TagTypeId<T1>.Id)
               && Contains(TagTypeId<T2>.Id)
               && Contains(TagTypeId<T3>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T1, T2, T3, T4>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        where T4 : struct, ITag
    {
        return Contains(TagTypeId<T1>.Id)
               && Contains(TagTypeId<T2>.Id)
               && Contains(TagTypeId<T3>.Id)
               && Contains(TagTypeId<T4>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T1, T2, T3, T4, T5>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        where T4 : struct, ITag
        where T5 : struct, ITag
    {
        return Contains(TagTypeId<T1>.Id)
               && Contains(TagTypeId<T2>.Id)
               && Contains(TagTypeId<T3>.Id)
               && Contains(TagTypeId<T4>.Id)
               && Contains(TagTypeId<T5>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasAny(Tags tags)
    {
        return tags.HasAll(_tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasAny(ReadOnlySpan<int> tags)
    {
        foreach (var t in tags)
            if (Contains(t))
                return true;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasAll(Tags tags)
    {
        return tags.HasAll(_tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasAll(ReadOnlySpan<int> tags)
    {
        foreach (var t in tags)
            if (!Contains(t))
                return false;

        return true;
    }

    public IEnumerator<Tag> GetEnumerator()
    {
        for (var i = 0; i < _count; i++)
            yield return _tags[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ReadOnlySpan<int> AsSpan()
        => _tags.AsSpan(0, _count);

    public static implicit operator Tags(ReadOnlySpan<int> tags) => new(tags);
    public static implicit operator ReadOnlySpan<int>(Tags tags) => tags.AsSpan();
    public static implicit operator Tags(int[] tags) => new(tags);
}