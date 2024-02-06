FROM mcr.microsoft.com/dotnet/sdk AS build
WORKDIR /src

COPY ./Todo.Api/Todo.Api.csproj ./Todo.Api/
COPY ./Todo.Core/Todo.Core.csproj ./Todo.Core/
RUN dotnet restore Todo.Api/Todo.Api.csproj

COPY ./Todo.Api ./Todo.Api
COPY ./Todo.Core ./Todo.Core/
RUN dotnet build Todo.Api/Todo.Api.csproj -c Release --no-restore

FROM build AS publish
RUN dotnet publish Todo.Api/Todo.Api.csproj  -c Release -o /app/publish --no-restore

# Use the base image and copy the published app into it
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Todo.Api.dll"]
