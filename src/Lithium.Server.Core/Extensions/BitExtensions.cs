namespace Lithium.Server.Core.Extensions;

public static class BitExtensions
{
    /// <summary>
    /// Sets a 4-bit nibble in a byte array.
    /// </summary>
    public static void SetNibble(this byte[] data, int index, byte value)
    {
        int fieldIdx = index >> 1;
        int shift = (index & 1) << 2; // 0 or 4
        
        // Clear the nibble at the position
        // If index is even (0), shift is 0. We want to keep the high nibble (0xF0).
        // If index is odd (1), shift is 4. We want to keep the low nibble (0x0F).
        // Java: val = (byte)(val & 15 << (i << 2)); -> This looks wrong/weird in the source?
        // Let's re-derive logic.
        // Even index (0): Low nibble. Shift=0. Mask=0xF0.
        // Odd index (1): High nibble. Shift=4. Mask=0x0F.
        
        // Java logic:
        // i = idx & 1; (0 or 1)
        // b = (b & 15) << ((i ^ 1) << 2); 
        //   If i=0 (even): (0^1)<<2 = 4. Shift value left by 4. So even index = High nibble?
        //   If i=1 (odd):  (1^1)<<2 = 0. Shift value left by 0. So odd index = Low nibble?
        // This is Little Endian packing? 
        // byte[0] = (nibble0 << 4) | nibble1 ?
        
        // Let's stick strictly to the Java implementation to ensure compatibility.
        /*
           int fieldIdx = idx >> 1;
           byte val = data[fieldIdx];
           b = (byte)(b & 15);
           int i = idx & 1;
           b = (byte)(b << ((i ^ 1) << 2));
           val = (byte)(val & 15 << (i << 2)); 
           val = (byte)(val | b);
           data[fieldIdx] = val;
        */
        
        int i = index & 1;
        value = (byte)(value & 0x0F);
        
        // i=0 -> shift=4. i=1 -> shift=0.
        int valueShift = (i ^ 1) << 2;
        value = (byte)(value << valueShift);
        
        // i=0 -> maskShift=0 -> mask=0x0F. We keep LOW bits? 
        // Wait, if we are setting the HIGH nibble (i=0, shift=4), we want to PRESERVE the LOW nibble.
        // 15 << 0 = 0x0F. Correct.
        // i=1 -> maskShift=4 -> mask=0xF0. We keep HIGH bits.
        // 15 << 4 = 0xF0. Correct.
        
        int maskShift = i << 2;
        int mask = 0x0F << maskShift;
        
        byte current = data[fieldIdx];
        current = (byte)(current & mask);
        current = (byte)(current | value);
        
        data[fieldIdx] = current;
    }

    /// <summary>
    /// Gets a 4-bit nibble from a byte array.
    /// </summary>
    public static byte GetNibble(this byte[] data, int index)
    {
        /*
           int fieldIdx = idx >> 1;
           byte val = data[fieldIdx];
           int i = idx & 1;
           val = (byte)(val >> ((i ^ 1) << 2));
           return (byte)(val & 15);
        */
        
        int fieldIdx = index >> 1;
        byte val = data[fieldIdx];
        int i = index & 1;
        
        // i=0 (even) -> shift=4. High nibble.
        // i=1 (odd)  -> shift=0. Low nibble.
        int shift = (i ^ 1) << 2;
        
        return (byte)((val >> shift) & 0x0F);
    }
}
