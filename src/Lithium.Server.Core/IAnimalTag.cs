using Lithium.Core.ECS;

namespace Lithium.Server.Core;

public interface IAnimalTag : ITag;

public readonly struct DogTag : IAnimalTag;

public readonly struct CatTag : IAnimalTag;