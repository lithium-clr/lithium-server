using System.Numerics;
using Hytale.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Hytale.Server.Core.Networking;

public sealed class EntityPositionHandler(ILogger<EntityPositionHandler> logger, IEntityManager entityManager)
    : IPacketHandler<EntityPositionPacket>
{
    void IPacketHandler<EntityPositionPacket>.Handle(in EntityPositionPacket p, PacketContext ctx)
    {
        logger.LogInformation("Set entity position to " + string.Join(", ", p.X, p.Y, p.Z));

        var entity = entityManager.Get(p.EntityId);
        entity?.Position = new Vector3(p.X, p.Y, p.Z);
    }
}