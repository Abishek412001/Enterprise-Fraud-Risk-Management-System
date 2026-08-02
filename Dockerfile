# Multi-stage Dockerfile for Enterprise Fraud Risk Management Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["backend/EnterpriseFraudRiskSystem.csproj", "backend/"]
RUN dotnet restore "backend/EnterpriseFraudRiskSystem.csproj"

COPY backend/ backend/
WORKDIR "/src/backend"
RUN dotnet build "EnterpriseFraudRiskSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnterpriseFraudRiskSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseFraudRiskSystem.dll"]
