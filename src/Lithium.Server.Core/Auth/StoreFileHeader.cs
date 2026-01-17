namespace Lithium.Server.Core.Auth;

public static class StoreFileHeader
{
    public static ReadOnlySpan<byte> MagicBytes => "LITH"u8;
    public const byte CurrentVersion = 1;
}