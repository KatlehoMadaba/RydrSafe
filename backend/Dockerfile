# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY RydrSafe.sln .
COPY RydrSafe.API/RydrSafe.API.csproj RydrSafe.API/
COPY RydrSafe.Application/RydrSafe.Application.csproj RydrSafe.Application/
COPY RydrSafe.Domain/RydrSafe.Domain.csproj RydrSafe.Domain/
COPY RydrSafe.Infrastructure/RydrSafe.Infrastructure.csproj RydrSafe.Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet publish RydrSafe.API/RydrSafe.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# OCR runs on the Google Cloud Vision API, not locally. The Tesseract native libraries that
# used to be installed here were never loaded by any code path — no package referenced them —
# so they only added image size and a misleading signal about where screenshots are processed.

COPY --from=build /app/publish .

EXPOSE 5000

# Use PORT env var injected by Render at runtime
CMD ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-5000} dotnet RydrSafe.API.dll"]
