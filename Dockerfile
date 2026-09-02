FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY . .
RUN dotnet restore OticaVisao.slnx
RUN dotnet publish src/OticaVisao.Web/OticaVisao.Web.csproj --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "exec dotnet OticaVisao.Web.dll --urls http://0.0.0.0:${PORT:-8080}"]
