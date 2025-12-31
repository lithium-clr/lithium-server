using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public interface ITag;

public readonly struct DisabledTag : ITag;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct Tag(int Id) : ITag
{
    public string Name => TagTypeId.GetName(Id);
    public Type Type => TagTypeId.GetType(Id);

    public static implicit operator Tag(int id) => new(id);
    public static implicit operator int(Tag tag) => tag.Id;
    public static implicit operator ReadOnlySpan<char>(Tag tag) => tag.Name;
    public static implicit operator Type(Tag tag) => tag.Type;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tag FromDefinition<T>()
        where T : struct, ITag
        => new(TagTypeId<T>.Id);

    public override string ToString() => Name;
}