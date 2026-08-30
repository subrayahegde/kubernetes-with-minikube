# Stage 1: Build
FROM "mcr.microsoft.com/dotnet/sdk:8.0" AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy remaining files and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM "mcr.microsoft.com/dotnet/sdk:8.0" AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Producer.dll"] 


