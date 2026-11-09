FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shop.csproj NuGet.Config ./
RUN dotnet restore
COPY Program.cs .
COPY site ./site
RUN dotnet publish -c Release --no-restore -o /out
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV HOST=0.0.0.0 PORT=8000
EXPOSE 8000
USER $APP_UID
ENTRYPOINT ["dotnet", "Shop.dll"]
