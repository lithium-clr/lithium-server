FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Game/Lithium.Server/Lithium.Server.csproj", "Game/Lithium.Server/"]
RUN dotnet restore "Game/Lithium.Server/Lithium.Server.csproj"
COPY . .
WORKDIR "/src/Game/Lithium.Server"
RUN dotnet build "./Lithium.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lithium.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lithium.Server.dll"]
