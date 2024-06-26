FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Microservices/ExternalApi/ExternalApi.csproj", "Microservices/ExternalApi/"]
COPY ["Libs/ApplicationUtils/ApplicationUtils.csproj", "Libs/ApplicationUtils/"]
RUN dotnet restore "Microservices/ExternalApi/ExternalApi.csproj"
COPY . .
WORKDIR "/src/Microservices/ExternalApi"
RUN dotnet build "ExternalApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExternalApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExternalApi.dll"]
