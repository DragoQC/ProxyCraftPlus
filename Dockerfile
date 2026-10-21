FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY ProxyCraftPlus.csproj packages.lock.json global.json ./
COPY Build/ Build/
RUN dotnet restore ProxyCraftPlus.csproj --locked-mode
COPY . .
RUN dotnet publish ProxyCraftPlus.csproj --configuration Release --no-restore \
    --no-self-contained -p:UseAppHost=false --output /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    Storage__Directory=/var/lib/proxycraftplus \
    DataProtection__KeysDirectory=/var/lib/proxycraftplus/keys \
    Http__RedirectToHttps=false
RUN mkdir -p /var/lib/proxycraftplus && chown app:app /var/lib/proxycraftplus
COPY --from=build /out/ ./
USER app
EXPOSE 8080/tcp 25565/tcp
ENTRYPOINT ["dotnet", "ProxyCraftPlus.dll"]
