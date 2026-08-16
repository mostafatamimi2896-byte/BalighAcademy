FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish BalighAcademy.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080;http://0.0.0.0:10000
EXPOSE 8080
ENTRYPOINT ["dotnet", "BalighAcademy.Api.dll"]
