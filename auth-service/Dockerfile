# ─── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Presentation/API/API.csproj", "src/Presentation/API/"]
COPY ["src/Core/Application/Application.csproj", "src/Core/Application/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Infrastructure/DependencyInjection/DependencyInjection.csproj", "src/Infrastructure/DependencyInjection/"]

RUN dotnet restore "src/Presentation/API/API.csproj"

COPY . .
WORKDIR "/src/src/Presentation/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# ─── Stage 2: Publish ─────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish --no-restore

# ─── Stage 3: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
