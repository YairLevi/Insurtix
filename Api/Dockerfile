# Build .NET API in Release mode, run via slim aspnet runtime.
#
# Bookstore file path:
#   Default: /app/data/bookstore.xml
#   Override: -e Bookstore__FilePath=/path/to/file.xml
#
# Listening port: 8080 (set via ASPNETCORE_URLS).
#   Override: -e ASPNETCORE_URLS=http://+:5000
#
# CORS: appsettings.json allows http://localhost by default.
#   Override: -e Cors__AllowedOrigins__0=http://my-frontend
#
# Build:  docker build -f Api/Dockerfile -t insurtix-api .
# Run:    docker run -p 8080:8080 insurtix-api

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
COPY Api/ Api/
RUN dotnet publish Api/Api.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Bookstore__FilePath=/app/data/bookstore.xml
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
