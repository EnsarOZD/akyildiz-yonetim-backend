# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "AkyildizYonetim.API/AkyildizYonetim.API.csproj"
RUN dotnet publish "AkyildizYonetim.API/AkyildizYonetim.API.csproj" -c Release -o /app/publish

# Çalıştırma aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AkyildizYonetim.API.dll"]