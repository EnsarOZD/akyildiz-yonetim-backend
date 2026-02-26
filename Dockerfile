# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY ["AkyildizYonetim.API/AkyildizYonetim.API.csproj", "AkyildizYonetim.API/"]
COPY ["AkyildizYonetim.Application/AkyildizYonetim.Application.csproj", "AkyildizYonetim.Application/"]
COPY ["AkyildizYonetim.Domain/AkyildizYonetim.Domain.csproj", "AkyildizYonetim.Domain/"]
COPY ["AkyildizYonetim.Infrastructure/AkyildizYonetim.Infrastructure.csproj", "AkyildizYonetim.Infrastructure/"]


# Restore dependencies
RUN dotnet restore "AkyildizYonetim.API/AkyildizYonetim.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish
RUN dotnet build "AkyildizYonetim.API/AkyildizYonetim.API.csproj" -c Release -o /app/build
RUN dotnet publish "AkyildizYonetim.API/AkyildizYonetim.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Install necessary packages for Alpine
RUN apk add --no-cache icu-libs

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:8080

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "AkyildizYonetim.API.dll"]