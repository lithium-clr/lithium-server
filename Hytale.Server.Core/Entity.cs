using System.Numerics;

namespace Hytale.Server.Core;

public abstract class Entity
{
    public int Id { get; set; }
    public Vector3 Position { get; internal set; }
}