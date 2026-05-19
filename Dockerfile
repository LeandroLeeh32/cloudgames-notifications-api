# BUILD STAGE

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copia solution
COPY *.sln .

# Copia projetos necessários
COPY CloudGames.Notifications.Functions/*.csproj ./CloudGames.Notifications.Functions/
COPY CloudGames.Notifications.Application/*.csproj ./CloudGames.Notifications.Application/
COPY CloudGames.Notifications.Domain/*.csproj ./CloudGames.Notifications.Domain/
COPY CloudGames.Notifications.Infrastructure/*.csproj ./CloudGames.Notifications.Infrastructure/

# Restore
RUN dotnet restore CloudGames.Notifications.Functions/CloudGames.Notifications.Functions.csproj

# Copia restante dos arquivos
COPY . .

# Define diretório da Function
WORKDIR /src/CloudGames.Notifications.Functions

# Publish
RUN dotnet publish -c Release -o /home/site/wwwroot

# RUNTIME STAGE

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

WORKDIR /home/site/wwwroot

COPY --from=build /home/site/wwwroot .

# Variáveis necessárias para Azure Functions
ENV AzureWebJobsScriptRoot=/home/site/wwwroot FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

# Porta utilizada pelo runtime
EXPOSE 80