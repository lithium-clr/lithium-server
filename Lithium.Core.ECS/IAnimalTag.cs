namespace Lithium.Core.ECS;

public interface IAnimalTag : ITag;

public readonly struct DogTag : IAnimalTag;

public readonly struct CatTag : IAnimalTag;