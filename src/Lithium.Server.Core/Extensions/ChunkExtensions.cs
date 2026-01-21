namespace Lithium.Server.Core.Extensions;

public static class ChunkExtensions
{
    // Constants from ChunkUtil
    public const int ChunkBits = 5;
    public const int ChunkSize = 32;
    public const int ChunkSizeMask = 31;
    public const int ChunkHeight = 320;

    /// <summary>
    /// Gets the chunk coordinate for a given block coordinate.
    /// </summary>
    public static int ToChunkCoordinate(this int blockCoord)
    {
        return blockCoord >> ChunkBits;
    }

    /// <summary>
    /// Gets the local block coordinate (0-31) within a chunk.
    /// </summary>
    public static int ToLocalCoordinate(this int blockCoord)
    {
        return blockCoord & ChunkSizeMask;
    }
    
    /// <summary>
    /// Gets the chunk index (long key) from chunk coordinates.
    /// </summary>
    public static long GetChunkIndex(int chunkX, int chunkZ)
    {
        return (long)chunkX << 32 | (uint)chunkZ;
    }

    /// <summary>
    /// Gets the X chunk coordinate from a chunk index.
    /// </summary>
    public static int GetChunkX(long index)
    {
        return (int)(index >> 32);
    }

    /// <summary>
    /// Gets the Z chunk coordinate from a chunk index.
    /// </summary>
    public static int GetChunkZ(long index)
    {
        return (int)index;
    }

    /// <summary>
    /// Checks if a global block coordinate is on the border of a chunk.
    /// </summary>
    public static bool IsChunkBorder(int x, int z)
    {
        int lx = x & ChunkSizeMask;
        int lz = z & ChunkSizeMask;
        return lx == 0 || lz == 0 || lx == 31 || lz == 31;
    }
    
    /// <summary>
    /// Calculates a linear index for a block within a chunk column (32x32xHeight).
    /// </summary>
    public static int GetBlockIndex(int localX, int worldY, int localZ)
    {
        // (y & HEIGHT_MASK) << 10 | (z & 31) << 5 | x & 31
        // Note: Java used HEIGHT_MASK derived from 320. 
        // 320 is not a power of 2, so strictly masking might wrap oddly if not careful?
        // Java: HEIGHT_MASK = (Integer.highestOneBit(320) << 1) - 1; -> 511 (0x1FF)
        // 512 is the next power of 2.
        
        return (worldY & 0x1FF) << 10 | (localZ & 31) << 5 | (localX & 31);
    }
}
