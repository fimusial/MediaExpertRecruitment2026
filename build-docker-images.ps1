docker image build --file ".\EzCatalog.API\WebAPI.Dockerfile" ".\EzCatalog.API" --tag ezcatalog-api
docker image build --file ".\EzCatalog.SPA\SPA.Server.Dockerfile" ".\EzCatalog.SPA" --tag ezcatalog-spa

New-Item -ItemType Directory -Path "$env:USERPROFILE\.aspnet\https" -Force | Out-Null
dotnet dev-certs https --export-path "$env:USERPROFILE\.aspnet\https\ezcatalog-spa.pfx" --password "ezcatalog-dev"
