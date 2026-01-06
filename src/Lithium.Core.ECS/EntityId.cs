using System.Runtime.InteropServices;

namespace Lithium.Core.ECS;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct EntityId
{
    private readonly int _id;

    public EntityId(int id)
    {
        _id = id;
    }

    public static implicit operator int(EntityId id) => id._id;
    public static implicit operator EntityId(int id) => new(id);

    public override string ToString() => $"{nameof(EntityId)}({_id})";
}