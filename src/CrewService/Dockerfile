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
COPY ["CrewService/CrewService.csproj", "CrewService/"]

# Restore dependencies (this will resolve project references)
RUN dotnet restore "CrewService/CrewService.csproj"

# Copy all Shared.Infrastructure source files
COPY ["Shared.Infrastructure/", "Shared.Infrastructure/"]

# Copy all remaining service source files
COPY ["CrewService/", "CrewService/"]

# Build and publish
WORKDIR "/src/CrewService"
RUN dotnet build "CrewService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/CrewService"
RUN dotnet publish "CrewService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "CrewService.dll"]

