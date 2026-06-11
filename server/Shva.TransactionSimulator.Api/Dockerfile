# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shva.TransactionSimulator.Api/Shva.TransactionSimulator.Api.csproj Shva.TransactionSimulator.Api/
RUN dotnet restore Shva.TransactionSimulator.Api/Shva.TransactionSimulator.Api.csproj
COPY . .
RUN dotnet publish Shva.TransactionSimulator.Api/Shva.TransactionSimulator.Api.csproj -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Shva.TransactionSimulator.Api.dll"]
