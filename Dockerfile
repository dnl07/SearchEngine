FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SearchEngine.Api/SearchEngine.Api.csproj", "SearchEngine.Api/"]
COPY ["SearchEngine.Core/SearchEngine.Core.csproj", "SearchEngine.Core/"]
COPY ["SearchEngine.Models/SearchEngine.Models.csproj", "SearchEngine.Models/"]

RUN dotnet restore "SearchEngine.Api/SearchEngine.Api.csproj"

COPY ["SearchEngine.Api/", "SearchEngine.Api/"]
COPY ["SearchEngine.Core/", "SearchEngine.Core/"]
COPY ["SearchEngine.Models/", "SearchEngine.Models/"]

RUN dotnet publish "SearchEngine.Api/SearchEngine.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:GenerateAssemblyInfo=false \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT [ "dotnet", "SearchEngine.Api.dll" ] 