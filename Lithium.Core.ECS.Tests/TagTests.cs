namespace Lithium.Core.ECS.Tests;

public class TagTests
{
    #region Constructor & Basic Properties

    [Fact]
    public void Constructor_WithNegativeId_ShouldSetId()
    {
        // Arrange & Act
        var tag = new Tag(-1);

        // Assert
        Assert.Equal(-1, tag.Id);
    }

    [Fact]
    public void Constructor_WithMaxIntId_ShouldSetId()
    {
        // Arrange & Act
        var tag = new Tag(int.MaxValue);

        // Assert
        Assert.Equal(int.MaxValue, tag.Id);
    }

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
        var tag = Tag.New<TestTag1>();

        // Assert
        Assert.Equal(TagTypeId<TestTag1>.Id, tag.Id);
    }

    [Fact]
    public void Equals_WithSameTag_ShouldReturnTrue()
    {
        // Arrange
        var tag1 = Tag.New<TestTag1>();
        var tag2 = Tag.New<TestTag1>();

        // Act & Assert
        Assert.True(tag1.Equals(tag2));
        Assert.True(tag1 == tag2);
        Assert.False(tag1 != tag2);
    }

    [Fact]
    public void Equals_WithDifferentTags_ShouldReturnFalse()
    {
        // Arrange
        var tag1 = Tag.New<TestTag1>();
        var tag2 = Tag.New<TestTag2>();

        // Act & Assert
        Assert.False(tag1.Equals(tag2));
        Assert.False(tag1 == tag2);
        Assert.True(tag1 != tag2);
    }

    [Fact]
    public void GetHashCode_ForEqualTags_ShouldBeEqual()
    {
        // Arrange
        var tag1 = Tag.New<TestTag1>();
        var tag2 = Tag.New<TestTag1>();

        // Act & Assert
        Assert.Equal(tag1.GetHashCode(), tag2.GetHashCode());
    }

    [Fact]
    public void Name_ShouldReturnTypeName()
    {
        // Arrange
        var tag = Tag.New<TestTag1>();

        // Act & Assert
        Assert.Equal(nameof(TestTag1), tag.Name);
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        var tag = Tag.New<TestTag1>();

        // Act & Assert
        Assert.Equal(tag.Name, tag.ToString());
    }

    #endregion

    #region Name and String Conversion

    [Fact]
    public void GetNameAsSpan_ShouldReturnCorrectName()
    {
        // Arrange
        var tag = Tag.New<TestTag1>();

        // Act
        var nameSpan = tag.GetNameAsSpan();

        // Assert
        Assert.Equal(nameof(TestTag1), nameSpan);
    }

    [Fact]
    public void GetNameAsString_ShouldReturnCorrectName()
    {
        // Arrange
        var tag = Tag.New<TestTag1>();

        // Act
        var name = tag.GetNameAsString();

        // Assert
        Assert.Equal(nameof(TestTag1), name);
    }

    [Fact]
    public void ToString_WithCustomTag_ShouldReturnCorrectName()
    {
        // Arrange
        var tag = Tag.New<DisabledTag>();

        // Act & Assert
        Assert.Equal(nameof(DisabledTag), tag.ToString());
    }

    #endregion

    #region Equality & Comparison

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var tag = Tag.New<TestTag1>();

        // Act & Assert
        Assert.False(tag.Equals(null));
    }

    [Fact]
    public void Equals_WithNonTagObject_ShouldReturnFalse()
    {
        // Arrange
        var tag = Tag.New<TestTag1>();
        var obj = new object();

        // Act & Assert
        Assert.False(tag.Equals(obj));
    }

    [Fact]
    public void GetHashCode_ForDifferentTags_ShouldBeDifferent()
    {
        // Arrange
        var tag1 = Tag.New<TestTag1>();
        var tag2 = Tag.New<TestTag2>();

        // Act & Assert
        Assert.NotEqual(tag1.GetHashCode(), tag2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ForSameTagDifferentInstance_ShouldBeEqual()
    {
        // Arrange
        var tag1 = new Tag(42);
        var tag2 = new Tag(42);

        // Act & Assert
        Assert.Equal(tag1.GetHashCode(), tag2.GetHashCode());
    }

    #endregion
}