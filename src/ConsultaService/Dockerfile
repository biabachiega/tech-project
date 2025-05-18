FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 9010
EXPOSE 9011

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConsultaService/ConsultaService.csproj", "ConsultaService/"]
RUN dotnet restore "ConsultaService/ConsultaService.csproj"
COPY . .
WORKDIR "/src/ConsultaService"
RUN dotnet build "ConsultaService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsultaService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsultaService.dll"]
