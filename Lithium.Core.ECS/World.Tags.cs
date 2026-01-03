using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private ISparseSet[] _tagSets = new ISparseSet[32];
    private Tags[] _entityTags = new Tags[1024];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTag<T>(Entity entity) where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;

        if (id >= _tagSets.Length)
        {
            var newSize = Math.Max(id + 1, _tagSets.Length * 2);
            Array.Resize(ref _tagSets, newSize);
        }
        
        _tagSets[id] ??= new SparseSet<T>();

        EnsureEntityTagsSize((int)entity.Id);
        
        ((SparseSet<T>)_tagSets[id]).Add(entity, default);
        _entityTags[entity.Id].Add(id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveTag<T>(Entity entity) where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;
        if (id >= _tagSets.Length) return;

        ((SparseSet<T>)_tagSets[id]).Remove(entity);
        
        if (entity.Id < _entityTags.Length)
            _entityTags[entity.Id].Remove(id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasTag<T>(Entity entity) where T : struct, ITag
    {
        if (entity.Id >= _entityTags.Length) return false;
        return _entityTags[entity.Id].Has(TagTypeId<T>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAllTags(Entity entity, ReadOnlySpan<int> tagIds)
    {
        if (entity.Id >= _entityTags.Length) return false;
        
        foreach (var id in tagIds)
            if (!_entityTags[entity.Id].Has(id))
                return false;

        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAllTags(Entity entity, in TagBitset mask)
    {
        if (entity.Id >= _entityTags.Length) return false;
        return _entityTags[entity.Id].HasAll(mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyTag(Entity entity, ReadOnlySpan<int> tagIds)
    {
        if (entity.Id >= _entityTags.Length) return false;

        foreach (var id in tagIds)
            if (_entityTags[entity.Id].Has(id))
                return true;

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyTag(Entity entity, in TagBitset mask)
    {
        if (entity.Id >= _entityTags.Length) return false;
        return _entityTags[entity.Id].HasAny(mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tags GetTags(Entity entity)
    {
        if (entity.Id >= _entityTags.Length) return Tags.Empty;
        return _entityTags[entity.Id];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<EntityId> GetEntitiesWithTag<T>() where T : struct, ITag
    {
        var id = TagTypeId<T>.Id;
        if (id >= _tagSets.Length || _tagSets[id] == null)
            return ReadOnlySpan<EntityId>.Empty;
            
        return _tagSets[id].Entities;
    }
    
    private void EnsureEntityTagsSize(int id)
    {
        if (id < _entityTags.Length) return;
        var newSize = Math.Max(id + 1, _entityTags.Length * 2);
        Array.Resize(ref _entityTags, newSize);
    }
}