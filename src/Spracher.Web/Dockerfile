FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /source

COPY . .
RUN dotnet restore src/Spracher.Web/Spracher.Web.csproj
RUN dotnet workload install wasm-tools
RUN dotnet publish src/Spracher.Web/Spracher.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM nginxinc/nginx-unprivileged:1.30.3-alpine AS final
COPY deploy/docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 8080
