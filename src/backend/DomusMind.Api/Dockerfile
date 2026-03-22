# Build context: repository root (e.g., /path/to/repo-root)
# docker build -f src/backend/DomusMind.Api/Dockerfile -t domusmind-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first — restore layer is cached until any .csproj changes
COPY src/backend/DomusMind.Api/DomusMind.Api.csproj                         src/backend/DomusMind.Api/
COPY src/backend/DomusMind.Application/DomusMind.Application.csproj         src/backend/DomusMind.Application/
COPY src/backend/DomusMind.Contracts/DomusMind.Contracts.csproj             src/backend/DomusMind.Contracts/
COPY src/backend/DomusMind.Domain/DomusMind.Domain.csproj                   src/backend/DomusMind.Domain/
COPY src/backend/DomusMind.Infrastructure/DomusMind.Infrastructure.csproj   src/backend/DomusMind.Infrastructure/

RUN dotnet restore src/backend/DomusMind.Api/DomusMind.Api.csproj

# Copy source and publish
COPY src/backend/ src/backend/
RUN dotnet publish src/backend/DomusMind.Api/DomusMind.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# HTTP only — TLS is terminated by the reverse proxy / docker host
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DomusMind.Api.dll"]
