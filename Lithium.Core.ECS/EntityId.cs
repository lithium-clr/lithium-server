using System.Runtime.InteropServices;

namespace Lithium.Core.ECS;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct EntityId
{
    private readonly uint _id;

    public EntityId(uint id)
    {
        _id = id;
    }

    public static implicit operator uint(EntityId id) => id._id;
    public static implicit operator EntityId(uint id) => new(id);

    public override string ToString() => $"{nameof(EntityId)}({_id})";
}