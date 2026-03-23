# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia a solution
COPY *.sln .

# Copia apenas projetos necessários
COPY CloudGames.Notifications.API/*.csproj ./CloudGames.Notifications.API/
COPY CloudGames.Notifications.Application/*.csproj ./CloudGames.Notifications.Application/
COPY CloudGames.Notifications.Domain/*.csproj ./CloudGames.Notifications.Domain/
COPY CloudGames.Notifications.Infrastructure/*.csproj ./CloudGames.Notifications.Infrastructure/

# Restore
RUN dotnet restore CloudGames.Notifications.API/CloudGames.Notifications.API.csproj

# Copia o restante do código
COPY . .

# Define diretório da API
WORKDIR /app/CloudGames.Notifications.API

# Publish
RUN dotnet publish -c Release -o /app/publish

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CloudGames.Notifications.API.dll"]