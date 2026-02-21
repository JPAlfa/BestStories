FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first to leverage Docker layer caching for restore
COPY BestStories.Api/BestStories.Api.csproj BestStories.Api/
COPY BestStories.Application/BestStories.Application.csproj BestStories.Application/
COPY BestStories.Domain/BestStories.Domain.csproj BestStories.Domain/
COPY BestStories.Infrastructure/BestStories.Infrastructure.csproj BestStories.Infrastructure/
RUN dotnet restore BestStories.Api/BestStories.Api.csproj

COPY BestStories.Api/ BestStories.Api/
COPY BestStories.Application/ BestStories.Application/
COPY BestStories.Domain/ BestStories.Domain/
COPY BestStories.Infrastructure/ BestStories.Infrastructure/
RUN dotnet publish BestStories.Api/BestStories.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BestStories.Api.dll"]
