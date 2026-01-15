namespace Lithium.Codecs;

/// <summary>
/// Apply this attribute to a class to automatically generate a codec implementation for it during compilation.
/// Remember, the class or you're targeting must be declared as `partial`.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CodecAttribute : Attribute;