# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["MessageAggregator/MessageAggregator.csproj", "MessageAggregator/"]
RUN dotnet restore "WebApi/WebApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/WebApi"
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
