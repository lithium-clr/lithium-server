namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<Type, ISparseSet> _tags = new();

    public void AddTag<T>(Entity entity, T tag) where T : struct, ITag
    {
        GetOrCreateTagSet<T>().Add(entity, tag);
    }

    public bool HasTag<T>(Entity entity) where T : struct, ITag
    {
        return _tags.TryGetValue(typeof(T), out var set) && ((SparseSet<T>)set).Has(entity);
    }

    public void RemoveTag<T>(Entity entity) where T : struct, ITag
    {
        if (_tags.TryGetValue(typeof(T), out var set))
            ((SparseSet<T>)set).Remove(entity);
    }

    private SparseSet<T> GetOrCreateTagSet<T>() where T : struct, ITag
    {
        if (_tags.TryGetValue(typeof(T), out var obj))
            return (SparseSet<T>)obj;

        var set = new SparseSet<T>();
        _tags[typeof(T)] = set;
        return set;
    }
}