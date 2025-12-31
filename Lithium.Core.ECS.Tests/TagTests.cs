namespace Lithium.Core.ECS.Tests;

public class TagTests
{
    [Fact]
    public void Constructor_WithValidId_ShouldSetId()
    {
        // Arrange & Act
        var tag = new Tag(42);

        // Assert
        Assert.Equal(42, tag.Id);
    }

    [Fact]
    public void New_WhenCalled_ShouldReturnTagWithCorrectId()
    {
        // Act
        var tag = Tag.New<DogTag>();

        // Assert
        Assert.Equal(TagTypeId<DogTag>.Id, tag.Id);
    }

    [Fact]
    public void Equals_WithSameTag_ShouldReturnTrue()
    {
        // Arrange
        var tag1 = Tag.New<DogTag>();
        var tag2 = Tag.New<DogTag>();

        // Act & Assert
        Assert.True(tag1.Equals(tag2));
        Assert.True(tag1 == tag2);
        Assert.False(tag1 != tag2);
    }

    [Fact]
    public void Equals_WithDifferentTags_ShouldReturnFalse()
    {
        // Arrange
        var tag1 = Tag.New<DogTag>();
        var tag2 = Tag.New<CatTag>();

        // Act & Assert
        Assert.False(tag1.Equals(tag2));
        Assert.False(tag1 == tag2);
        Assert.True(tag1 != tag2);
    }

    [Fact]
    public void GetHashCode_ForEqualTags_ShouldBeEqual()
    {
        // Arrange
        var tag1 = Tag.New<DogTag>();
        var tag2 = Tag.New<DogTag>();

        // Act & Assert
        Assert.Equal(tag1.GetHashCode(), tag2.GetHashCode());
    }

    [Fact]
    public void Name_ShouldReturnTypeName()
    {
        // Arrange
        var tag = Tag.New<DogTag>();
    
        // Act & Assert
        Assert.Equal(nameof(DogTag), tag.Name);
    }
  
    [Fact]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        var tag = Tag.New<DogTag>();
    
        // Act & Assert
        Assert.Equal(tag.Name, tag.ToString());
    }
}
