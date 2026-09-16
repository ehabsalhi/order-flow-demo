FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PaymentService/PaymentService.csproj PaymentService/
RUN dotnet restore PaymentService/PaymentService.csproj

COPY PaymentService/ PaymentService/
RUN dotnet publish PaymentService/PaymentService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=
ENV ASPNETCORE_HTTPS_PORTS=
EXPOSE 3100
EXPOSE 3101
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]
