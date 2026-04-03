FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Hospital.sln", "./"]
COPY ["src/Hospital.Seeder/Hospital.Seeder.csproj", "src/Hospital.Seeder/"]

RUN dotnet restore "src/Hospital.Seeder/Hospital.Seeder.csproj"

COPY . .

RUN dotnet publish "src/Hospital.Seeder/Hospital.Seeder.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Hospital.Seeder.dll"]
