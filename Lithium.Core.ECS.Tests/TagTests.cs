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
    public void FromDefinition_WhenCalled_ShouldReturnTagWithCorrectId()
    {
        // Act
        var tag = Tag.FromDefinition<DogTag>();

        // Assert
        Assert.Equal(TagTypeId<DogTag>.Id, tag.Id);
    }

    [Fact]
    public void Equals_WithSameTag_ShouldReturnTrue()
    {
        // Arrange
        var tag1 = Tag.FromDefinition<DogTag>();
        var tag2 = Tag.FromDefinition<DogTag>();

        // Act & Assert
        Assert.True(tag1.Equals(tag2));
        Assert.True(tag1 == tag2);
        Assert.False(tag1 != tag2);
    }

    [Fact]
    public void Equals_WithDifferentTags_ShouldReturnFalse()
    {
        // Arrange
        var tag1 = Tag.FromDefinition<DogTag>();
        var tag2 = Tag.FromDefinition<CatTag>();

        // Act & Assert
        Assert.False(tag1.Equals(tag2));
        Assert.False(tag1 == tag2);
        Assert.True(tag1 != tag2);
    }

    [Fact]
    public void GetHashCode_ForEqualTags_ShouldBeEqual()
    {
        // Arrange
        var tag1 = Tag.FromDefinition<DogTag>();
        var tag2 = Tag.FromDefinition<DogTag>();

        // Act & Assert
        Assert.Equal(tag1.GetHashCode(), tag2.GetHashCode());
    }

    [Fact]
    public void Name_ShouldReturnTypeName()
    {
        // Arrange
        var tag = Tag.FromDefinition<DogTag>();

        // Act & Assert
        Assert.Equal(nameof(DogTag), tag.Name);
    }

    [Fact]
    public void Type_ShouldReturnCorrectType()
    {
        // Arrange
        var tag = Tag.FromDefinition<DogTag>();

        // Act & Assert
        Assert.Equal(typeof(DogTag), tag.Type);
    }

    [Fact]
    public void ImplicitConversion_FromInt_ShouldCreateCorrectTag()
    {
        // Arrange
        var id = TagTypeId<DogTag>.Id;

        // Act
        Tag tag = id;

        // Assert
        Assert.Equal(id, tag.Id);
    }

    [Fact]
    public void ImplicitConversion_ToInt_ShouldReturnId()
    {
        // Arrange
        var tag = Tag.FromDefinition<DogTag>();

        // Act
        int id = tag;

        // Assert
        Assert.Equal(tag.Id, id);
    }

    [Fact]
    public void ImplicitConversion_ToReadOnlySpan_ShouldReturnNameAsSpan()
    {
        // Arrange
        var tag = Tag.FromDefinition<DogTag>();

        // Act
        ReadOnlySpan<char> span = tag;

        // Assert
        Assert.Equal(tag.Name, span.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToType_ShouldReturnTagType()
    {
        // Arrange
        var tag = Tag.FromDefinition<DogTag>();

        // Act
        Type type = tag;

        // Assert
        Assert.Equal(tag.Type, type);
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        var tag = Tag.FromDefinition<DogTag>();

        // Act & Assert
        Assert.Equal(tag.Name, tag.ToString());
    }
}
