FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MainServer/MainServer.csproj MainServer/
RUN dotnet restore MainServer/MainServer.csproj

COPY MainServer/ MainServer/
RUN dotnet publish MainServer/MainServer.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MainServer.dll"]
