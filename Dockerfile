FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copia tudo da pasta FutebolApi
COPY . .

# 🔴 IMPORTANTE: aponta explicitamente o csproj
RUN dotnet restore "Futebol.Api.csproj"

# publica a API
RUN dotnet publish "Futebol.Api.csproj" -c Release -o /app/publish

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Futebol.Api.dll"]