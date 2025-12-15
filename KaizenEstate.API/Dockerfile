
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["KaizenEstate.API/KaizenEstate.API.csproj", "KaizenEstate.API/"]
COPY ["KaizenEstate.Shared/KaizenEstate.Shared.csproj", "KaizenEstate.Shared/"]

# Восстанавливаем зависимости для API
RUN dotnet restore "KaizenEstate.API/KaizenEstate.API.csproj"

COPY . .

WORKDIR "/src/KaizenEstate.API"
RUN dotnet publish "KaizenEstate.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "KaizenEstate.API.dll"]