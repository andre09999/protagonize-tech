FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/TaskManager.Api/TaskManager.Api.csproj backend/TaskManager.Api/
RUN dotnet restore backend/TaskManager.Api/TaskManager.Api.csproj

COPY backend/TaskManager.Api/ backend/TaskManager.Api/
RUN dotnet publish backend/TaskManager.Api/TaskManager.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
