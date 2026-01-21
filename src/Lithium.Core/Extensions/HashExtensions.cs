namespace Lithium.Core.Extensions;

public static class HashExtensions
{
    /// <summary>
    /// Hashes a 64-bit integer using a custom avalanche function.
    /// Matches Hytale's HashUtil.hash(long).
    /// </summary>
    public static long Hash(long v)
    {
        // v = (v >>> 30 ^ v) * -4658895280553007687L;
        // v = (v >>> 27 ^ v) * -7723592293110705685L;
        // return v >>> 31 ^ v;
        
        // In C#, >>> is >>> for ulong, or >> for long (arithmetic shift).
        // Java's >>> is logical shift (zero fill).
        // So we must cast to ulong for logical shifts.

        ulong uv = (ulong)v;
        uv = (uv >> 30 ^ uv) * 0xBF58476D1CE4E5B9UL; // -4658895280553007687L as ulong
        uv = (uv >> 27 ^ uv) * 0x94D049BB133111EBUL; // -7723592293110705685L as ulong
        return (long)(uv >> 31 ^ uv);
    }

    /// <summary>
    /// Hashes two 64-bit integers.
    /// </summary>
    public static long Hash(long l1, long l2)
    {
        ulong ul1 = (ulong)l1;
        ul1 = (ul1 >> 30 ^ ul1) * 0xBF58476D1CE4E5B9UL;
        
        long h2 = Hash(l2);
        return (long)((ulong)h2 >> 31 ^ ul1);
    }

    /// <summary>
    /// Hashes three 64-bit integers.
    /// </summary>
    public static long Hash(long l1, long l2, long l3)
    {
        ulong ul1 = (ulong)l1;
        ul1 = (ul1 >> 30 ^ ul1) * 0xBF58476D1CE4E5B9UL;
        
        ulong h2 = (ulong)Hash(l2);
        ul1 = (h2 >> 27 ^ ul1) * 0x94D049BB133111EBUL;
        
        long h3 = Hash(l3);
        return (long)((ulong)h3 >> 31 ^ ul1);
    }
    
    /// <summary>
    /// Rehashes a value (hashes it twice).
    /// </summary>
    public static long Rehash(long l1) => Hash(Hash(l1));

    /// <summary>
    /// Generates a deterministic random double [0, 1) from a seed.
    /// </summary>
    public static double RandomDouble(long seed)
    {
        long h = Rehash(seed);
        // hash &= 4294967295L (0xFFFFFFFF)
        // return hash / 4.294967295E9;
        
        long masked = h & 0xFFFFFFFFL;
        return masked / 4294967295.0;
    }
    
    /// <summary>
    /// Generates a deterministic random double [0, 1) from two seeds.
    /// </summary>
    public static double RandomDouble(long l1, long l2)
    {
        long h = Hash(Hash(l1, l2));
        long masked = h & 0xFFFFFFFFL;
        return masked / 4294967295.0;
    }
}
