using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    // index = TagTypeId
    private ISparseSet[] _tagSets = new ISparseSet[32];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTag<T>(Entity entity)
        where T : struct, ITag
        => GetOrCreateTagSet<T>().Add(entity, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveTag<T>(Entity entity)
        where T : struct, ITag
        => GetTagSet<T>()?.Remove(entity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasTag<T>(Entity entity)
        where T : struct, ITag
        => GetTagSet<T>()?.Has(entity) ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAllTags(Entity entity, ReadOnlySpan<int> tagIds)
    {
        foreach (var id in tagIds)
        {
            var set = id < _tagSets.Length ? _tagSets[id] : null;
            
            if (set is null || !set.Has(entity))
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyTag(Entity entity, ReadOnlySpan<int> tagIds)
    {
        foreach (var id in tagIds)
        {
            var set = id < _tagSets.Length ? _tagSets[id] : null;
            
            if (set is not null && set.Has(entity))
                return true;
        }

        return false;
    }

    public int[] GetTags(Entity entity)
    {
        var count = 0;

        for (var i = 0; i < _tagSets.Length; i++)
            if (_tagSets[i]?.Has(entity) == true)
                count++;

        if (count == 0)
            return [];

        var result = new int[count];
        var idx = 0;

        for (var i = 0; i < _tagSets.Length; i++)
            if (_tagSets[i]?.Has(entity) == true)
                result[idx++] = i;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SparseSet<T>? GetTagSet<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;
        return id < _tagSets.Length ? (SparseSet<T>?)_tagSets[id] : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SparseSet<T> GetOrCreateTagSet<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;

        if (id >= _tagSets.Length)
            Array.Resize(ref _tagSets, id * 2);

        var set = _tagSets[id];

        if (set is not null)
            return (SparseSet<T>)set;

        var created = new SparseSet<T>();
        _tagSets[id] = created;
        return created;
    }
}