namespace BestStories.Application.DTOs;

public record HackerNewsItemResponse(
    string Title,
    string Url,
    string By,
    long Time,
    int Score,
    int Descendants
);
