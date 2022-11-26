FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY src /src
WORKDIR /src
RUN dotnet restore 
RUN dotnet build SupibotTelegramProxy.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish SupibotTelegramProxy.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN rm -rf /src

ENTRYPOINT ["dotnet", "SupibotTelegramProxy.dll"]