# Multi-stage Dockerfile for Unison REST Adapter
# TASK-009: CI/CD Pipeline Setup - Container Support

# =====================================
# STAGE 1: BUILD
# =====================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Unison Access Service REST API.sln", "./"]
COPY ["UnisonRestAdapter/UnisonRestAdapter.csproj", "UnisonRestAdapter/"]
COPY ["UnisonRestAdapter.Tests/UnisonRestAdapter.Tests.csproj", "UnisonRestAdapter.Tests/"]

# Restore dependencies
RUN dotnet restore "Unison Access Service REST API.sln"

# Copy all source code
COPY . .

# Build and test
WORKDIR "/src/UnisonRestAdapter"
RUN dotnet build "UnisonRestAdapter.csproj" -c Release -o /app/build

# Run tests
WORKDIR "/src"
RUN dotnet test "UnisonRestAdapter.Tests/UnisonRestAdapter.Tests.csproj" -c Release --no-build --verbosity normal

# Publish application
WORKDIR "/src/UnisonRestAdapter"
RUN dotnet publish "UnisonRestAdapter.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =====================================
# STAGE 2: RUNTIME
# =====================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN useradd -m -s /bin/bash appuser && \
    chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=build --chown=appuser:appuser /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Set entry point
ENTRYPOINT ["dotnet", "UnisonRestAdapter.dll"]
