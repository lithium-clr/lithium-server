using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public interface ITag;

public readonly struct DisabledTag : ITag;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct Tag(int id) : IEquatable<Tag>
{
    public readonly int Id = id;

    public string Name => GetNameAsString();
    
    public static Tag New<T>() where T : struct, ITag
    {
        return new Tag(TagTypeId<T>.Id);
    }
    
    public ReadOnlySpan<char> GetNameAsSpan() => TagTypeId.GetName(Id);
    public string GetNameAsString() => TagTypeId.GetNameString(Id);
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Tag other) => Id == other.Id;

    public override bool Equals(object? obj)
    {
        return obj is Tag tag && Equals(tag);
    }
    
    public override int GetHashCode() => Id;

    public override string ToString() => new(TagTypeId.GetName(Id));
    
    public static bool operator ==(Tag left, Tag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Tag left, Tag right)
    {
        return !(left == right);
    }
}