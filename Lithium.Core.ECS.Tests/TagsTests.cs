namespace Lithium.Core.ECS.Tests;

public class TagsTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WhenDefault_ShouldCreateEmptyTags()
    {
        // Act
        var tags = new Tags();

        // Assert
        Assert.Empty(tags);
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
        var initialCount = tags.Count;

        // Act
        tags.Add<DogTag>();

        // Assert
        Assert.Equal(initialCount, tags.Count);
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
        Assert.True(tags.Has(otherTags));
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
        var (dogTag, catTag) = tags.Get<DogTag, CatTag>();

        // Assert
        Assert.Equal(TagTypeId<DogTag>.Id, dogTag.Id);
        Assert.Equal(TagTypeId<CatTag>.Id, catTag.Id);
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

    #region Other Functionality Tests

    [Fact]
    public void AsSpan_ShouldReturnTagsAsSpan()
    {
        var tags = new Tags();
        
        // Arrange
        tags.Add<DogTag>();
        tags.Add<CatTag>();
        
        Span<int> buffer = stackalloc int[TagBitset.BitsPerBlock];
        
        var count = tags.AsSpan(buffer);
        var ids = buffer[..count].ToArray();

        // Assert
        Assert.Equal(2, count);
        Assert.Contains(TagTypeId<DogTag>.Id, ids);
        Assert.Contains(TagTypeId<CatTag>.Id, ids);
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

    #endregion
}