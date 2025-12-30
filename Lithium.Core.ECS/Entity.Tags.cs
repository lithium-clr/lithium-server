namespace Lithium.Core.ECS;

public partial record struct Entity
{
    public void AddTag<T>() where T : struct, ITag
    {
        World.AddTag<T>(this);
    }
    
    public void RemoveTag<T>() where T : struct, ITag
    {
        World.RemoveTag<T>(this);
    }
    
    public bool HasTag<T>() where T : struct, ITag
    {
        return World.HasTag<T>(this);
    }
    
    public bool HasAnyTag(ReadOnlySpan<int> tagIds)
    {
        return World.HasAnyTag(this, tagIds);
    }
    
    public bool HasAllTags(ReadOnlySpan<int> tagIds)
    {
        return World.HasAllTags(this, tagIds);
    }
}