FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY common.props common.props
COPY KV.Server.sln KV.Server.sln
COPY src/KV.Server.Application/KV.Server.Application.csproj src/KV.Server.Application/KV.Server.Application.csproj
COPY src/KV.Server.Application.Contracts/KV.Server.Application.Contracts.csproj src/KV.Server.Application.Contracts/KV.Server.Application.Contracts.csproj
COPY src/KV.Server.DbMigrator/KV.Server.DbMigrator.csproj src/KV.Server.DbMigrator/KV.Server.DbMigrator.csproj
COPY src/KV.Server.Domain/KV.Server.Domain.csproj src/KV.Server.Domain/KV.Server.Domain.csproj
COPY src/KV.Server.Domain.Shared/KV.Server.Domain.Shared.csproj src/KV.Server.Domain.Shared/KV.Server.Domain.Shared.csproj
COPY src/KV.Server.EntityFrameworkCore/KV.Server.EntityFrameworkCore.csproj src/KV.Server.EntityFrameworkCore/KV.Server.EntityFrameworkCore.csproj
COPY src/KV.Server.HttpApi/KV.Server.HttpApi.csproj src/KV.Server.HttpApi/KV.Server.HttpApi.csproj
COPY src/KV.Server.HttpApi.Client/KV.Server.HttpApi.Client.csproj src/KV.Server.HttpApi.Client/KV.Server.HttpApi.Client.csproj
COPY src/KV.Server.Web/KV.Server.Web.csproj src/KV.Server.Web/KV.Server.Web.csproj

RUN dotnet restore src/KV.Server.Web/KV.Server.Web.csproj

ARG GITLAB_NPM_TOKEN

COPY . .
COPY src/KV.Server.Web/wwwroot/plugins/bootstrap/js/bootstrap.enable.tooltips.everywhere.js src/KV.Server.Web/wwwroot/libs/bootstrap/js/bootstrap.enable.tooltips.everywhere.js

FROM build AS publish
RUN dotnet publish src/KV.Server.Web/KV.Server.Web.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "KV.Server.Web.dll"]