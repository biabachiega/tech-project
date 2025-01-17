FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y curl

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApplication1/ProjetoTech.csproj", "WebApplication1/"]
RUN dotnet restore "./WebApplication1/ProjetoTech.csproj"
COPY . .
WORKDIR "/src/WebApplication1"
RUN dotnet build "./ProjetoTech.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Install PostgreSQL Client in the build stage
RUN apt-get update && apt-get install -y postgresql-client

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProjetoTech.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjetoTech.dll"]
