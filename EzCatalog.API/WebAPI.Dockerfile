FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# TODO: best dockerfile practices

WORKDIR /source
COPY ./Domain ./Domain
COPY ./Application ./Application
COPY ./Infrastructure ./Infrastructure
COPY ./WebAPI ./WebAPI

WORKDIR /source/WebAPI
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 7171
ENV ASPNETCORE_HTTP_PORTS=7171
ENTRYPOINT ["dotnet", "WebAPI.dll"]