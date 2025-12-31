namespace Lithium.Core.ECS.Tests;

public class TagsTests
{
    #region Setup & Helpers

    private static Tags CreateTagsWithSampleData()
    {
        var tags = new Tags();
        tags.Add<DogTag>();
        tags.Add<CatTag>();
        return tags;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenDefault_ShouldCreateEmptyTags()
    {
        // Act
        var tags = new Tags();

        // Assert
        Assert.Empty(tags);
    }

    [Fact]
    public void Constructor_WithReadOnlySpan_ShouldCreateTagsFromSpan()
    {
        // Arrange
        var span = new[] { TagTypeId<DogTag>.Id, TagTypeId<CatTag>.Id };

        // Act
        var tags = new Tags(span);

        // Assert
        Assert.True(tags.Has<DogTag>());
        Assert.True(tags.Has<CatTag>());
    }

    #endregion

    #region Add & Remove Tests

    [Fact]
    public void Add_WhenAddingNewTag_ShouldContainTag()
    {
        var tags = new Tags();
        
        // Act
        tags.Add<DogTag>();

        // Assert
        Assert.True(tags.Has<DogTag>());
    }

    [Fact]
    public void Add_WhenAddingDuplicateTag_ShouldNotAddDuplicate()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        var initialCount = tags.Count();

        // Act
        tags.Add<DogTag>();

        // Assert
        Assert.Equal(initialCount, tags.Count());
    }

    [Fact]
    public void Remove_WhenTagExists_ShouldRemoveTag()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        
        // Act
        tags.Remove<DogTag>();

        // Assert
        Assert.False(tags.Has<DogTag>());
    }

    // [Fact]
    // public void Remove_WhenTagDoesNotExist_ShouldNotThrow()
    // {
    //     var tags = new Tags();
    //     
    //     // Act & Assert (should not throw)
    //     tags.Remove<DogTag>();
    // }

    #endregion

    #region Has Tests

    [Fact]
    public void Has_WithSingleTag_WhenTagExists_ShouldReturnTrue()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();

        // Act & Assert
        Assert.True(tags.Has<DogTag>());
    }

    [Fact]
    public void Has_WithMultipleTags_WhenAllTagsExist_ShouldReturnTrue()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();

        // Act & Assert
        Assert.True(tags.Has<DogTag, CatTag>());
    }

    [Fact]
    public void Has_WithMultipleTags_WhenNotAllTagsExist_ShouldReturnFalse()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();

        // Act & Assert
        Assert.False(tags.Has<DogTag, CatTag>());
    }

    #endregion

    #region HasAll Tests

    [Fact]
    public void Contains_WithTags_WhenAllTagsExist_ShouldReturnTrue()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();
        
        var otherTags = new Tags();
        otherTags.Add<DogTag>();

        // Act & Assert
        Assert.True(tags.Contains(otherTags));
    }

    [Fact]
    public void HasAll_WithReadOnlySpan_WhenAllTagsExist_ShouldReturnTrue()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        
        var span = new[] { TagTypeId<DogTag>.Id };

        // Act & Assert
        Assert.True(tags.HasAll(span));
    }

    #endregion

    #region HasAny Tests

    [Fact]
    public void HasAny_WithTags_WhenAnyTagExists_ShouldReturnTrue()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        
        var otherTags = new Tags();
        otherTags.Add<DogTag>();
        otherTags.Add<CatTag>();

        // Act & Assert
        Assert.True(tags.HasAny(otherTags));
    }

    [Fact]
    public void HasAny_WithReadOnlySpan_WhenAnyTagExists_ShouldReturnTrue()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        
        var span = new[] { TagTypeId<DogTag>.Id, TagTypeId<CatTag>.Id };

        // Act & Assert
        Assert.True(tags.HasAny(span));
    }

    #endregion

    #region Get Tests

    [Fact]
    public void Get_WithSingleTag_ShouldReturnTag()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();

        // Act
        var tag = tags.Get<DogTag>();

        // Assert
        Assert.Equal(TagTypeId<DogTag>.Id, tag.Id);
    }

    [Fact]
    public void Get_WithMultipleTags_ShouldReturnAllTags()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();

        // Act
        var result = tags.Get<DogTag, CatTag>();

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(TagTypeId<DogTag>.Id, result[0].Id);
        Assert.Equal(TagTypeId<CatTag>.Id, result[1].Id);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_WhenIndexInRange_ShouldReturnTag()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();

        // Act & Assert
        Assert.Equal(TagTypeId<DogTag>.Id, tags[0].Id);
        Assert.Equal(TagTypeId<CatTag>.Id, tags[1].Id);
    }

    [Fact]
    public void Indexer_WhenIndexOutOfRange_ShouldThrow()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => tags[1]);
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void GetEnumerator_ShouldEnumerateAllTags()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();
        
        var expectedIds = new HashSet<int> { TagTypeId<DogTag>.Id, TagTypeId<CatTag>.Id };
        var actualIds = new HashSet<int>();

        // Act
        foreach (var tag in tags)
        {
            actualIds.Add(tag.Id);
        }

        // Assert
        Assert.Equal(expectedIds, actualIds);
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ImplicitConversion_FromReadOnlySpan_ShouldCreateTags()
    {
        // Arrange
        var span = new[] { TagTypeId<DogTag>.Id, TagTypeId<CatTag>.Id };

        // Act
        Tags tags = span;

        // Assert
        Assert.True(tags.Has<DogTag>());
        Assert.True(tags.Has<CatTag>());
    }

    [Fact]
    public void ImplicitConversion_ToReadOnlySpan_ShouldReturnTagsAsSpan()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();

        // Act
        ReadOnlySpan<int> span = tags;

        // Assert
        Assert.Equal(2, span.Length);
        Assert.Contains(TagTypeId<DogTag>.Id, span.ToArray());
        Assert.Contains(TagTypeId<CatTag>.Id, span.ToArray());
    }

    [Fact]
    public void ImplicitConversion_FromIntArray_ShouldCreateTags()
    {
        // Arrange
        var array = new[] { TagTypeId<DogTag>.Id, TagTypeId<CatTag>.Id };

        // Act
        Tags tags = array;

        // Assert
        Assert.True(tags.Has<DogTag>());
        Assert.True(tags.Has<CatTag>());
    }

    #endregion

    #region Other Functionality Tests

    [Fact]
    public void AsSpan_ShouldReturnTagsAsSpan()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();

        // Act
        var span = tags.AsSpan();

        // Assert
        Assert.Equal(2, span.Length);
        Assert.Contains(TagTypeId<DogTag>.Id, span.ToArray());
        Assert.Contains(TagTypeId<CatTag>.Id, span.ToArray());
    }

    [Fact]
    public void Empty_ShouldReturnEmptyTags()
    {
        // Act
        var empty = Tags.Empty;

        // Assert
        // Assert.NotNull(empty);
        Assert.Empty(empty);
    }

    [Fact]
    public void Add_WhenExceedingInitialCapacity_ShouldResizeArray()
    {
        var tags = new Tags();
        
        // Arrange & Act
        for (var i = 0; i < 10; i++)
            tags.Add(TagTypeId<DogTag>.Id + i);

        // Assert
        Assert.Equal(10, tags.Count());
    }


    #endregion
}