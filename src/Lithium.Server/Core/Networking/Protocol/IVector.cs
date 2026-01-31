using System.Numerics;

namespace Lithium.Server.Core.Networking.Protocol;

public interface IVector<T> : IFormattable
    where T : INumber<T>;