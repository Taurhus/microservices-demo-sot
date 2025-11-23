FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Shared.Infrastructure project file and source files (needed for project reference resolution)
COPY ["Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared.Infrastructure/"]
COPY ["Shared.Infrastructure/", "Shared.Infrastructure/"]

# Copy service project file
COPY ["QuestService/QuestService.csproj", "QuestService/"]

# Restore dependencies (this will resolve project references)
RUN dotnet restore "QuestService/QuestService.csproj"

# Copy all remaining service source files
COPY ["QuestService/", "QuestService/"]

# Build and publish
WORKDIR "/src/QuestService"
RUN dotnet build "QuestService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/QuestService"
RUN dotnet publish "QuestService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "QuestService.dll"]
