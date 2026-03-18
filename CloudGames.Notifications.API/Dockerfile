FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia a solution
COPY *.sln .

# Copia TODOS os projetos
COPY CloudGames.Notifications.API/*.csproj ./CloudGames.Notifications.API/
COPY CloudGames.Notifications.Application/*.csproj ./CloudGames.Notifications.Application/
COPY CloudGames.Notifications.Domain/*.csproj ./CloudGames.Notifications.Domain/
COPY CloudGames.Notifications.Infrastructure/*.csproj ./CloudGames.Notifications.Infrastructure/
COPY CloudGames.Notifications.Tests/*.csproj ./CloudGames.Notifications.Tests/

# Restore
RUN dotnet restore

# Copia o restante do código
COPY . .

# Build
RUN dotnet publish CloudGames.Notifications.API -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CloudGames.Notifications.API.dll"]