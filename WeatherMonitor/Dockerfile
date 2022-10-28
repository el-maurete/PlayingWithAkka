FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /src/
COPY *.sln .
COPY WeatherMonitor/*.csproj WeatherMonitor/
RUN dotnet restore
COPY . .
RUN dotnet publish --configuration Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app
COPY --from=build /app /app
ENTRYPOINT ["dotnet", "WeatherMonitor.dll"]
