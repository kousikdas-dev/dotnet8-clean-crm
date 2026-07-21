# ==========================
# Stage 1 - Build
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore CRM.api.sln

RUN dotnet publish CRM.api/CRM.api.csproj -c Release -o /app/publish

# ==========================
# Stage 2 - Runtime
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CRM.api.dll"]