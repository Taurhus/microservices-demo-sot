FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Shared.Infrastructure project file first
COPY ["Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared.Infrastructure/"]

# Copy service project file
COPY ["ReputationService/ReputationService.csproj", "ReputationService/"]

# Restore dependencies (this will resolve project references)
RUN dotnet restore "ReputationService/ReputationService.csproj"

# Copy all Shared.Infrastructure source files
COPY ["Shared.Infrastructure/", "Shared.Infrastructure/"]

# Copy all remaining service source files
COPY ["ReputationService/", "ReputationService/"]

# Build and publish
WORKDIR "/src/ReputationService"
RUN dotnet build "ReputationService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/ReputationService"
RUN dotnet publish "ReputationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "ReputationService.dll"]

