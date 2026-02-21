# Best Stories API

ASP.NET Core Web API that retrieves the top *n* "best stories" from the [Hacker News API](https://github.com/HackerNews/API), sorted by score in descending order.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## How to Run

```bash
# Clone the repository
git clone https://github.com/<your-username>/BestStories.git
cd BestStories

# Build
dotnet build

# Run
dotnet run --project BestStories.Api
```

The API will be available at `http://localhost:5242`.

### API Documentation

In development mode, interactive API docs are available via [Scalar](https://scalar.com/) at:

```
http://localhost:5242/scalar/v1
```

## Usage

```bash
# Get the top 5 best stories
curl http://localhost:5242/api/beststories?n=5
```

### Response Format

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/ABlockOrigin/uBlock-issues/issues/745",
    "postedBy": "AsmailDonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  },
  ...
]
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `n` | int (query) | Number of stories to return. Must be > 0. |

| Status | Description |
|--------|-------------|
| 200 OK | Returns JSON array of stories sorted by score descending |
| 400 Bad Request | Invalid or missing `n` parameter |

## Running Tests

```bash
dotnet test
```

- **26 tests** total: 14 unit tests + 5 integration tests + 7 domain tests

## Architecture

The project follows **Clean Architecture** with clear dependency direction:

```
Domain ← Application ← Infrastructure ← Api
```

| Project | Responsibility |
|---------|---------------|
| **BestStories.Domain** | Rich domain entity (`Story`) with invariants and factory method |
| **BestStories.Application** | Use case orchestration, gateway interface, DTOs |
| **BestStories.Infrastructure** | Hacker News HTTP client, in-memory caching |
| **BestStories.Api** | REST controller, DI configuration, startup |

### Key Design Decisions

- **In-memory caching** — All ~200 best stories are fetched once and cached (configurable TTL, default 5 min). Any value of *n* is served from the same cache.
- **Throttled parallelism** — Story details are fetched concurrently using `SemaphoreSlim` (configurable, default max 20) to avoid overloading the Hacker News API.
- **Rich Domain Model** — `Story` validates its own invariants in the constructor (non-empty title, non-negative score, etc.) and converts Unix timestamps to ISO 8601.
- **UseCase pattern** — Application layer orchestrates the flow: fetches raw data via gateway interface → creates domain entities → sorts → limits.
- **DependencyInjection extensions** — Each layer registers its own services via `AddApplication()` / `AddInfrastructure()`.

## Configuration

All settings are in `BestStories.Api/appsettings.json`:

```json
{
  "HackerNews": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0/",
    "CacheTtlMinutes": 5,
    "MaxConcurrency": 20
  }
}
```

## Assumptions

- The Hacker News API is available and returns valid JSON.
- Stories may have an empty `url` field (e.g., "Ask HN" posts) — these are returned with an empty `uri`.
- The `descendants` field from the HN API maps to `commentCount`.
- Cache is shared across all requests — the first request after cache expiration triggers a full refresh.

## Possible Enhancements

- **Distributed cache** — Replace `IMemoryCache` with Redis for multi-instance deployments.
- **Resilience policies** — Add Polly for retries, circuit breaker, and timeout on HN API calls.
- **Health check endpoint** — `GET /health` to verify HN API connectivity.
- **Pagination** — Support `offset` / `limit` instead of just `n`.
- **Rate limiting** — Add ASP.NET Core rate limiting middleware to protect the API.
- **Logging** — Add structured logging with Serilog for better observability.
- **Docker support** — Add `Dockerfile` and `docker-compose.yml` for containerized deployment.
