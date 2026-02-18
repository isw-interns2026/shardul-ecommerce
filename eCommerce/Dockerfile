# Backend Dockerfile — multi-stage build
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore (cached layer — only re-runs when dependencies change)
COPY eCommerce/ECommerce.csproj eCommerce/
RUN dotnet restore eCommerce/ECommerce.csproj

# Copy everything and publish
COPY eCommerce/ eCommerce/
RUN dotnet publish eCommerce/ECommerce.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime (no SDK, no source code, minimal image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Npgsql needs Kerberos libs for GSSAPI support
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/Logs

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ECommerce.dll"]
