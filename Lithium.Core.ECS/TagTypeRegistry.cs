using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

internal static class TagTypeRegistry
{
    private static int _next;

    private static readonly Dictionary<Type, int> TypeToId = new(64);
    private static readonly List<Type> IdToType = new(64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetOrCreate(Type type)
    {
        if (TypeToId.TryGetValue(type, out var id))
            return id;

        id = _next++;
        TypeToId[type] = id;
        IdToType.Add(type);

        return id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type GetType(int id)
        => IdToType[id];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(int id)
        => IdToType[id].Name;
}