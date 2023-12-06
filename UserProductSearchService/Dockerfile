FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UserProductSearchService/UserProductSearchService.csproj", "UserProductSearchService/"]
RUN dotnet restore "UserProductSearchService/UserProductSearchService.csproj"
COPY . .
WORKDIR "/src/UserProductSearchService"
RUN dotnet build "UserProductSearchService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserProductSearchService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProductSearchService.dll"]
