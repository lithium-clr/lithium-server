using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private ISparseSet[] _tagSets = new ISparseSet[32];
    private Tags[] _entityTags = new Tags[32];

    // // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTag<T>(Entity entity) where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;

        if (id >= _tagSets.Length)
        {
            var newSize = Math.Max(id + 1, _tagSets.Length * 2);

            Array.Resize(ref _tagSets, newSize);
            Array.Resize(ref _entityTags, newSize);
        }
        
        _tagSets[id] ??= new SparseSet<T>();

        if (_entityTags[entity.Id].Equals(default))
            _entityTags[entity.Id] = new Tags();
        
        ((SparseSet<T>)_tagSets[id]).Add(entity, default);
        _entityTags[entity.Id].Add(id);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveTag<T>(Entity entity) where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;
        if (id >= _tagSets.Length) return;

        ((SparseSet<T>)_tagSets[id]).Remove(entity);
        _entityTags[entity.Id].Remove(id);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasTag<T>(Entity entity) where T : struct, ITag
    {
        return _entityTags[entity.Id].Has(TagTypeId<T>.Id);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAllTags(Entity entity, ReadOnlySpan<int> tagIds)
    {
        foreach (var id in tagIds)
            if (!_entityTags[entity.Id].Has(id))
                return false;

        return true;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyTag(Entity entity, ReadOnlySpan<int> tagIds)
    {
        foreach (var id in tagIds)
            if (_entityTags[entity.Id].Has(id))
                return true;

        return false;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tags GetTags(Entity entity) => _entityTags[entity.Id];

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<EntityId> GetEntitiesWithTag<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;
        return _tagSets[id].Entities;
    }
}