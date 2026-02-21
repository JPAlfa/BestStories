using BestStories.Domain;

namespace BestStories.Tests.Domain;

public class StoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateStory()
    {
        var story = Story.Create("Test Title", "https://example.com", "user1", 1571922181, 100, 50);

        Assert.Equal("Test Title", story.Title);
        Assert.Equal("https://example.com", story.Uri);
        Assert.Equal("user1", story.PostedBy);
        Assert.Equal(100, story.Score);
        Assert.Equal(50, story.CommentCount);
    }

    [Fact]
    public void Create_ShouldConvertUnixTimestampToIso8601()
    {
        var story = Story.Create("Title", "https://example.com", "user1", 1571922181, 10, 5);

        Assert.Contains("2019-10-24", story.Time);
    }

    [Fact]
    public void Create_WithNullUrl_ShouldSetUriToEmpty()
    {
        var story = Story.Create("Title", null!, "user1", 1571922181, 10, 5);

        Assert.Equal(string.Empty, story.Uri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ShouldThrow(string? title)
    {
        Assert.Throws<ArgumentException>(() =>
            Story.Create(title!, "https://example.com", "user1", 1571922181, 10, 5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPostedBy_ShouldThrow(string? postedBy)
    {
        Assert.Throws<ArgumentException>(() =>
            Story.Create("Title", "https://example.com", postedBy!, 1571922181, 10, 5));
    }

    [Fact]
    public void Create_WithNegativeScore_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Story.Create("Title", "https://example.com", "user1", 1571922181, -1, 5));
    }

    [Fact]
    public void Create_WithNegativeCommentCount_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Story.Create("Title", "https://example.com", "user1", 1571922181, 10, -1));
    }
}
