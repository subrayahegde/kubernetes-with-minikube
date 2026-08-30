# File: Producer.Dockerfile & Consumer.Dockerfile
FROM "mcr.microsoft.com/dotnet/sdk:8.0" AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Producer.csproj", "./"]
RUN dotnet restore "./Producer.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "Producer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM "mcr.microsoft.com/dotnet/sdk:8.0" AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Producer.dll"]
