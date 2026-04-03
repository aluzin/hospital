FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Hospital.sln", "./"]
COPY ["src/Hospital.Api/Hospital.Api.csproj", "src/Hospital.Api/"]
COPY ["src/Hospital.Application/Hospital.Application.csproj", "src/Hospital.Application/"]
COPY ["src/Hospital.Domain/Hospital.Domain.csproj", "src/Hospital.Domain/"]
COPY ["src/Hospital.Infrastructure/Hospital.Infrastructure.csproj", "src/Hospital.Infrastructure/"]

RUN dotnet restore "src/Hospital.Api/Hospital.Api.csproj"

COPY . .

RUN dotnet publish "src/Hospital.Api/Hospital.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Hospital.Api.dll"]
