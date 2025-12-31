using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial record struct Entity
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddTag<T>() where T : struct, ITag
        => World.AddTag<T>(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveTag<T>() where T : struct, ITag
        => World.RemoveTag<T>(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasTag<T>() where T : struct, ITag
        => World.HasTag<T>(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyTag(ReadOnlySpan<int> tagIds)
        => World.HasAnyTag(this, tagIds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAllTags(ReadOnlySpan<int> tagIds)
        => World.HasAllTags(this, tagIds);

    public int[] GetTags() => World.GetTags(this);
}