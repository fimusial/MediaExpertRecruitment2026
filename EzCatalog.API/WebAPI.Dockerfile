FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY Directory.Build.props ./
COPY Domain/Domain.csproj ./Domain/
COPY Application/Application.csproj ./Application/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY WebAPI/WebAPI.csproj ./WebAPI/

RUN dotnet restore WebAPI/WebAPI.csproj

COPY Domain/ ./Domain/
COPY Application/ ./Application/
COPY Infrastructure/ ./Infrastructure/
COPY WebAPI/ ./WebAPI/

RUN dotnet publish WebAPI/WebAPI.csproj -c Release -o /app --no-restore -p:OpenApiGenerateDocuments=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app ./

USER $APP_UID
EXPOSE 7171
ENV ASPNETCORE_HTTP_PORTS=7171
ENTRYPOINT ["dotnet", "WebAPI.dll"]
