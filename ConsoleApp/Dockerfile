FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ConsoleApp/ConsoleApp.csproj ConsoleApp/
RUN dotnet restore ConsoleApp/ConsoleApp.csproj
COPY . .
WORKDIR /src/ConsoleApp
RUN dotnet build ConsoleApp.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ConsoleApp.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ConsoleApp.dll"]
