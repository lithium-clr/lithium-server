namespace Lithium.Core;

public readonly record struct ClientId(ulong Id)
{
    public static implicit operator ulong(ClientId id) => id.Id;
    public static implicit operator ClientId(ulong id) => new(id);
}