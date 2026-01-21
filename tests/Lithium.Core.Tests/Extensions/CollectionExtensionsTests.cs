using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class CollectionExtensionsTests
{
    [Fact]
    public void PickRandom_ShouldReturnElement_WhenCollectionIsNotEmpty()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var random = list.PickRandom();
        list.Should().Contain(random);
    }

    [Fact]
    public void PickRandom_ShouldReturnDefault_WhenCollectionIsEmpty()
    {
        var list = new List<int>();
        list.PickRandom().Should().Be(0);
        
        var strList = new List<string>();
        strList.PickRandom().Should().BeNull();
    }
    
    [Fact]
    public void PickRandom_ShouldWorkWithEnumerable()
    {
        IEnumerable<int> GetNumbers()
        {
            yield return 1;
            yield return 2;
        }
        
        var random = GetNumbers().PickRandom();
        new[] { 1, 2 }.Should().Contain(random);
    }
    
    [Fact]
    public void Shuffle_ShouldShuffleList()
    {
        // This test is probabilistic, but we can check if elements are preserved.
        var list = Enumerable.Range(0, 100).ToList();
        var original = new List<int>(list);
        
        list.Shuffle();
        
        list.Should().BeEquivalentTo(original);
        list.Should().NotEqual(original); // Extremely unlikely to be in same order
    }
}
