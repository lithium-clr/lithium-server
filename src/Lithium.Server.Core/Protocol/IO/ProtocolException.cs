namespace Lithium.Server.Core.Protocol.IO;

public class ProtocolException : Exception
{
    public ProtocolException(string message) : base(message)
    {
    }

    public ProtocolException(string message, Exception cause) : base(message, cause)
    {
    }

    public static ProtocolException ArrayTooLong(string fieldName, int actual, int max)
    {
        return new ProtocolException($"{fieldName}: array length {actual} exceeds maximum {max}");
    }

    public static ProtocolException StringTooLong(string fieldName, int actual, int max)
    {
        return new ProtocolException($"{fieldName}: string length {actual} exceeds maximum {max}");
    }

    public static ProtocolException DictionaryTooLarge(string fieldName, int actual, int max)
    {
        return new ProtocolException($"{fieldName}: dictionary count {actual} exceeds maximum {max}");
    }

    public static ProtocolException BufferTooSmall(string fieldName, int required, int available)
    {
        return new ProtocolException($"{fieldName}: buffer too small, need {required} bytes but only {available} available");
    }

    public static ProtocolException InvalidVarInt(string fieldName)
    {
        return new ProtocolException($"{fieldName}: invalid or incomplete VarInt");
    }

    public static ProtocolException NegativeLength(string fieldName, int value)
    {
        return new ProtocolException($"{fieldName}: negative length {value}");
    }

    public static ProtocolException InvalidOffset(string fieldName, int offset, int bufferLength)
    {
        return new ProtocolException($"{fieldName}: offset {offset} is out of bounds (buffer length: {bufferLength})");
    }

    public static ProtocolException UnknownPolymorphicType(string typeName, int typeId)
    {
        return new ProtocolException($"{typeName}: unknown polymorphic type ID {typeId}");
    }

    public static ProtocolException DuplicateKey(string fieldName, object key)
    {
        return new ProtocolException($"{fieldName}: duplicate key '{key}'");
    }

    public static ProtocolException InvalidEnumValue(string enumName, int value)
    {
        return new ProtocolException($"{enumName}: invalid enum value {value}");
    }

    public static ProtocolException ArrayTooShort(string fieldName, int actual, int min)
    {
        return new ProtocolException($"{fieldName}: array length {actual} is below minimum {min}");
    }

    public static ProtocolException StringTooShort(string fieldName, int actual, int min)
    {
        return new ProtocolException($"{fieldName}: string length {actual} is below minimum {min}");
    }

    public static ProtocolException DictionaryTooSmall(string fieldName, int actual, int min)
    {
        return new ProtocolException($"{fieldName}: dictionary count {actual} is below minimum {min}");
    }

    public static ProtocolException ValueOutOfRange(string fieldName, object value, double min, double max)
    {
        return new ProtocolException($"{fieldName}: value {value} is outside allowed range [{min}, {max}]");
    }

    public static ProtocolException ValueBelowMinimum(string fieldName, object value, double min)
    {
        return new ProtocolException($"{fieldName}: value {value} is below minimum {min}");
    }

    public static ProtocolException ValueAboveMaximum(string fieldName, object value, double max)
    {
        return new ProtocolException($"{fieldName}: value {value} exceeds maximum {max}");
    }
}