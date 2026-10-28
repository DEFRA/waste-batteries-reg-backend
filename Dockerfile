# Build stage image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src"
# `dotnet test` uses the testing-platform server and currently reports that zero
# tests ran. Launch the Debug assemblies directly, as scripts/run-tests.sh does.
RUN dotnet build WasteBatteriesRegBackend.slnx -c Debug \
 && dotnet tests/WasteBatteriesRegBackend.Tests.Unit/bin/Debug/net10.0/WasteBatteriesRegBackend.Tests.Unit.dll \
 && dotnet tests/WasteBatteriesRegBackend.Tests.Integration/bin/Debug/net10.0/WasteBatteriesRegBackend.Tests.Integration.dll
RUN dotnet publish src/WasteBatteriesRegBackend -c Release -o /app/publish /p:UseAppHost=false

# Final production image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

USER root
# Add curl to template, CDP PLATFORM HEALTHCHECK REQUIREMENT
RUN apt update && \
    apt install curl -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8085
ENTRYPOINT ["dotnet", "WasteBatteriesRegBackend.dll"]
