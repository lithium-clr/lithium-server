// using System.Runtime.CompilerServices;
//
// namespace Lithium.Core.Extensions;
//
// public static class UnmanagedExtensions
// {
//     extension<T>(T) where T : unmanaged
//     {
//         public static ushort SizeOf() => (ushort)Unsafe.SizeOf<T>();
//     }
//
//     extension<T>(T obj) where T : unmanaged
//     {
//         public ushort GetSize() => (ushort)Unsafe.SizeOf<T>();
//     }
// }