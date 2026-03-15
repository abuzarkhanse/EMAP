FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EMAP.sln", "./"]
COPY ["EMAP.Web/EMAP.Web.csproj", "EMAP.Web/"]
COPY ["EMAP.Domain/EMAP.Domain.csproj", "EMAP.Domain/"]
COPY ["EMAP.Infrastructure/EMAP.Infrastructure.csproj", "EMAP.Infrastructure/"]

RUN dotnet restore "EMAP.Web/EMAP.Web.csproj"

COPY . .
WORKDIR "/src/EMAP.Web"
RUN dotnet publish "EMAP.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "EMAP.Web.dll"]
