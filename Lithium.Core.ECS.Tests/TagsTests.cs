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
        // Arrange
        var tags = new Tags();

        // Act
        tags.Add<TestTag1>();

        // Assert
        Assert.True(tags.Has<TestTag1>());
        Assert.Equal(1, tags.Count);
    }

    [Fact]
    public void Add_WithMultipleTags_ShouldUpdateCountCorrectly()
    {
        // Arrange
        var tags = new Tags();

        // Act
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();
        tags.Add<TestTag3>();

        // Assert
        Assert.Equal(3, tags.Count);
    }

    [Fact]
    public void Add_WithIntId_ShouldAddTag()
    {
        // Arrange
        var tags = new Tags();
        var tagId = TagTypeId<TestTag1>.Id;

        // Act
        tags.Add(tagId);

        // Assert
        Assert.True(tags.Has(tagId));
    }

    [Fact]
    public void Add_WhenAddingDuplicateTag_ShouldNotAddDuplicate()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();
        var initialCount = tags.Count;

        // Act
        tags.Add<TestTag1>();

        // Assert
        Assert.Equal(initialCount, tags.Count);
    }

    [Fact]
    public void Remove_WhenTagExists_ShouldRemoveTag()
    {
        // Arrange
        var tags = new Tags();
        tags.Add<TestTag1>();

        // Act
        tags.Remove<TestTag1>();

        // Assert
        Assert.False(tags.Has<TestTag1>());
        Assert.Equal(0, tags.Count);
    }

    [Fact]
    public void Remove_WhenTagDoesNotExist_ShouldNotAffectCount()
    {
        // Arrange
        var tags = new Tags();
        tags.Add<TestTag1>();
        var initialCount = tags.Count;

        // Act
        tags.Remove<TestTag2>();

        // Assert
        Assert.Equal(initialCount, tags.Count);
    }

    [Fact]
    public void Remove_WithIntId_ShouldRemoveTag()
    {
        // Arrange
        var tags = new Tags();
        var tagId = TagTypeId<TestTag1>.Id;
        tags.Add(tagId);

        // Act
        tags.Remove(tagId);

        // Assert
        Assert.False(tags.Has(tagId));
    }

    #endregion

    #region Has Tests

    [Fact]
    public void Has_WithSingleTag_WhenTagExists_ShouldReturnTrue()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();

        // Act & Assert
        Assert.True(tags.Has<TestTag1>());
    }

    [Fact]
    public void Has_WithMultipleTags_WhenAllTagsExist_ShouldReturnTrue()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();

        // Act & Assert
        Assert.True(tags.Has<TestTag1, TestTag2>());
    }

    [Fact]
    public void Has_WithMultipleTags_WhenNotAllTagsExist_ShouldReturnFalse()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();

        // Act & Assert
        Assert.False(tags.Has<TestTag1, TestTag2>());
    }

    #endregion

    #region HasAll Tests

    [Fact]
    public void Contains_WithTags_WhenAllTagsExist_ShouldReturnTrue()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();

        var otherTags = new Tags();
        otherTags.Add<TestTag1>();

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
        tags.Add<TestTag1>();

        var otherTags = new Tags();
        otherTags.Add<TestTag1>();
        otherTags.Add<TestTag2>();

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
        tags.Add<TestTag1>();

        // Act
        var tag = tags.Get<TestTag1>();

        // Assert
        Assert.Equal(TagTypeId<TestTag1>.Id, tag.Id);
    }

    [Fact]
    public void Get_WithMultipleTags_ShouldReturnAllTags()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();

        // Act
        var (tag1, tag2) = tags.Get<TestTag1, TestTag2>();

        // Assert
        Assert.Equal(TagTypeId<TestTag1>.Id, tag1.Id);
        Assert.Equal(TagTypeId<TestTag2>.Id, tag2.Id);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_WhenIndexInRange_ShouldReturnTag()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();

        // Act & Assert
        Assert.Equal(TagTypeId<TestTag1>.Id, tags[0].Id);
        Assert.Equal(TagTypeId<TestTag2>.Id, tags[1].Id);
    }

    [Fact]
    public void Indexer_WhenIndexOutOfRange_ShouldThrow()
    {
        var tags = new Tags();

        // Arrange
        tags.Add<TestTag1>();

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
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();

        var expectedIds = new HashSet<int> { TagTypeId<TestTag1>.Id, TagTypeId<TestTag2>.Id };
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
        tags.Add<TestTag1>();
        tags.Add<TestTag2>();

        Span<int> buffer = stackalloc int[TagBitset.BitsPerBlock];

        var count = tags.AsSpan(buffer);
        var ids = buffer[..count].ToArray();

        // Assert
        Assert.Equal(2, count);
        Assert.Contains(TagTypeId<TestTag1>.Id, ids);
        Assert.Contains(TagTypeId<TestTag2>.Id, ids);
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