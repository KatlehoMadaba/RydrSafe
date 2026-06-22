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

# Install Tesseract OCR native libraries and English language data
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng \
    libtesseract-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Point to system tessdata installed by apt
ENV Tesseract__DataPath=/usr/share/tesseract-ocr/5/tessdata

EXPOSE 5000

# Use PORT env var injected by Render at runtime
CMD ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-5000} dotnet RydrSafe.API.dll"]
