FROM mcr.microsoft.com/dotnet/sdk AS build
WORKDIR /src

COPY ./Todo.UI/Todo.UI.csproj ./Todo.UI/
COPY ./Todo.Core/Todo.Core.csproj ./Todo.Core/
RUN dotnet restore Todo.UI/Todo.UI.csproj

COPY ./Todo.UI ./Todo.UI
COPY ./Todo.Core ./Todo.Core/
RUN dotnet build Todo.UI/Todo.UI.csproj -c Release

FROM build AS publish
RUN dotnet publish Todo.UI/Todo.UI.csproj  -c Release -o /app/publish

FROM nginx:latest AS final
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf