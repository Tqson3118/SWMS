# Stage 1: Build & Publish .NET 10 Minimal API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/Swms.Api.sln backend/Swms.Api.sln
COPY backend/src/Swms.Api/Swms.Api.csproj backend/src/Swms.Api/Swms.Api.csproj
COPY backend/tests/Swms.Tests/Swms.Tests.csproj backend/tests/Swms.Tests/Swms.Tests.csproj

RUN dotnet restore backend/src/Swms.Api/Swms.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/Swms.Api/Swms.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Ensure storage and document mount points exist
RUN mkdir -p /app/App_Data/storage/documents /app/document

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5080
ENV URLS=http://0.0.0.0:5080
ENV DocumentLibrary__RootPath=/app/document
ENV Storage__LocalStoragePath=/app/App_Data/storage/documents

EXPOSE 5080

ENTRYPOINT ["dotnet", "Swms.Api.dll"]
