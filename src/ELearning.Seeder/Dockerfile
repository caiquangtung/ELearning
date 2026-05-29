FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ELearning.Seeder/ELearning.Seeder.csproj", "ELearning.Seeder/"]
COPY ["ELearning.Infrastructure/ELearning.Infrastructure.csproj", "ELearning.Infrastructure/"]
COPY ["ELearning.Application/ELearning.Application.csproj", "ELearning.Application/"]
COPY ["ELearning.Domain/ELearning.Domain.csproj", "ELearning.Domain/"]
COPY ["ELearning.Core/ELearning.Core.csproj", "ELearning.Core/"]
RUN dotnet restore "ELearning.Seeder/ELearning.Seeder.csproj"
COPY . .
RUN dotnet publish "ELearning.Seeder/ELearning.Seeder.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ELearning.Seeder.dll"]
