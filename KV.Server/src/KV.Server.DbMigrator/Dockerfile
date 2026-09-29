FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

COPY common.props /repo/common.props

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
COPY src/KV.Server.PublicWeb/KV.Server.PublicWeb.csproj src/KV.Server.PublicWeb/KV.Server.PublicWeb.csproj

RUN dotnet restore /repo/src/KV.Server.DbMigrator/KV.Server.DbMigrator.csproj
COPY . .
RUN dotnet build /repo/src/KV.Server.DbMigrator/KV.Server.DbMigrator.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish /repo/src/KV.Server.DbMigrator/KV.Server.DbMigrator.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "KV.Server.DbMigrator.dll"]
