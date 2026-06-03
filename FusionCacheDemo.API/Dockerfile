FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj files and restore
COPY FusionCacheDemo.Domain/FusionCacheDemo.Domain.csproj FusionCacheDemo.Domain/
COPY FusionCacheDemo.Application/FusionCacheDemo.Application.csproj FusionCacheDemo.Application/
COPY FusionCacheDemo.Infrastructure/FusionCacheDemo.Infrastructure.csproj FusionCacheDemo.Infrastructure/
COPY FusionCacheDemo.API/FusionCacheDemo.API.csproj FusionCacheDemo.API/

RUN dotnet restore FusionCacheDemo.API/FusionCacheDemo.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish FusionCacheDemo.API/FusionCacheDemo.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FusionCacheDemo.API.dll"]
