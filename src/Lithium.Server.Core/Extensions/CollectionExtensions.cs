namespace Lithium.Server.Core.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Picks a random element from the collection.
    /// </summary>
    public static T? PickRandom<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        // Optimization for IList/IReadOnlyList to avoid full enumeration
        if (source is IReadOnlyList<T> list)
        {
            if (list.Count == 0) return default;
            return list[Random.Shared.Next(list.Count)];
        }
        
        // Fallback for generic enumerables (reservoir sampling or ToArray)
        var array = source.ToArray();
        if (array.Length == 0) return default;
        return array[Random.Shared.Next(array.Length)];
    }

    /// <summary>
    /// Shuffles the list in-place using the Fisher-Yates algorithm.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Shared.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
