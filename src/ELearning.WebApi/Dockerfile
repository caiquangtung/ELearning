FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ELearning.WebApi/ELearning.WebApi.csproj", "ELearning.WebApi/"]
COPY ["ELearning.Application/ELearning.Application.csproj", "ELearning.Application/"]
COPY ["ELearning.Infrastructure/ELearning.Infrastructure.csproj", "ELearning.Infrastructure/"]
COPY ["ELearning.Domain/ELearning.Domain.csproj", "ELearning.Domain/"]
COPY ["ELearning.Core/ELearning.Core.csproj", "ELearning.Core/"]
RUN dotnet restore "ELearning.WebApi/ELearning.WebApi.csproj"
COPY . .
RUN dotnet build "ELearning.WebApi/ELearning.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ELearning.WebApi/ELearning.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ELearning.WebApi.dll"]
