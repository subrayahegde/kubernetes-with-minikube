# Define the image versions explicitly at the top
ARG BASE_IMAGE="mcr.microsoft.com/dotnet/sdk:8.0"
ARG SDK_IMAGE="mcr.microsoft.com/dotnet/sdk:8.0"

FROM ${BASE_IMAGE} AS base
WORKDIR /app
EXPOSE 8080

FROM ${SDK_IMAGE} AS build
WORKDIR /src
COPY ["WebApi.csproj", "."]
RUN dotnet restore "WebApi.csproj"
COPY . .
RUN dotnet build "WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]

