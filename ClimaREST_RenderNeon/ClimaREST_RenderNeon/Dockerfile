# Compilación de la aplicación ASP.NET Core.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RestFulWebService.csproj", "./"]
RUN dotnet restore "RestFulWebService.csproj"
COPY . .
RUN dotnet publish "RestFulWebService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen de ejecución.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestFulWebService.dll"]
