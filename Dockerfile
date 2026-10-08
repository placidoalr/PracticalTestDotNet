FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["nuget.config", "./"]
COPY ["src/PracticalTestDotNet.Domain/PracticalTestDotNet.Domain.csproj", "src/PracticalTestDotNet.Domain/"]
COPY ["src/PracticalTestDotNet.Application/PracticalTestDotNet.Application.csproj", "src/PracticalTestDotNet.Application/"]
COPY ["src/PracticalTestDotNet.Infrastructure/PracticalTestDotNet.Infrastructure.csproj", "src/PracticalTestDotNet.Infrastructure/"]
COPY ["src/PracticalTestDotNet.Api/PracticalTestDotNet.Api.csproj", "src/PracticalTestDotNet.Api/"]

RUN dotnet restore "src/PracticalTestDotNet.Api/PracticalTestDotNet.Api.csproj"

COPY . .
WORKDIR "/src/src/PracticalTestDotNet.Api"
RUN dotnet build "PracticalTestDotNet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PracticalTestDotNet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PracticalTestDotNet.Api.dll"]
