namespace BestStories.Domain;

public class Story
{
    public string Title { get; }
    public string Uri { get; }
    public string PostedBy { get; }
    public string Time { get; }
    public int Score { get; }
    public int CommentCount { get; }

    private Story(string title, string url, string postedBy, long unixTime, int score, int commentCount)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be null or empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(postedBy))
            throw new ArgumentException("PostedBy must not be null or empty.", nameof(postedBy));

        if (score < 0)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be greater than or equal to zero.");

        if (commentCount < 0)
            throw new ArgumentOutOfRangeException(nameof(commentCount), "CommentCount must be greater than or equal to zero.");

        Title = title;
        Uri = url ?? string.Empty;
        PostedBy = postedBy;
        Time = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToString("yyyy-MM-ddTHH:mm:sszzz");
        Score = score;
        CommentCount = commentCount;
    }

    public static Story Create(string title, string url, string postedBy, long unixTime, int score, int commentCount)
    {
        return new Story(title, url, postedBy, unixTime, score, commentCount);
    }
}
