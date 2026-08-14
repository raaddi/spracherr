FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /source

COPY . .
RUN dotnet restore src/Spracher.Api/Spracher.Api.csproj
RUN dotnet publish src/Spracher.Api/Spracher.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER root
RUN mkdir -p /var/lib/spracher/dataprotection-keys \
    && chown -R $APP_UID:$APP_UID /var/lib/spracher

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Spracher.Api.dll"]
