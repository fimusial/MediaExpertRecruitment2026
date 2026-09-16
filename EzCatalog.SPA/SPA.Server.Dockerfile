FROM node:24-alpine AS spa-build
WORKDIR /source/ezcatalog-spa

COPY ezcatalog-spa/package.json ezcatalog-spa/package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY ezcatalog-spa/ ./

RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY EzCatalog.SPA.Server/EzCatalog.SPA.Server.csproj ./EzCatalog.SPA.Server/
RUN dotnet restore EzCatalog.SPA.Server/EzCatalog.SPA.Server.csproj

COPY EzCatalog.SPA.Server/ ./EzCatalog.SPA.Server/
COPY --from=spa-build /source/EzCatalog.SPA.Server/wwwroot/ ./EzCatalog.SPA.Server/wwwroot/

RUN dotnet publish EzCatalog.SPA.Server/EzCatalog.SPA.Server.csproj -c Release -o /app --no-restore -p:SkipSpaBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app ./

USER $APP_UID
EXPOSE 7200
ENV ASPNETCORE_HTTP_PORTS=7200
ENV ReverseProxy__Clusters__ezcatalog-api__Destinations__primary__Address=http://ezcatalog-api:7171/
ENTRYPOINT ["dotnet", "EzCatalog.SPA.Server.dll"]
