FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Product.WebApi/Product.WebApi.csproj", "Product.WebApi/"]
COPY ["Product.Application/Product.Application.csproj", "Product.Application/"]
COPY ["Product.Domain/Product.Domain.csproj", "Product.Domain/"]
COPY ["Product.Infrastructure/Product.Infrastructure.csproj", "Product.Infrastructure/"]
COPY ["Product.Seeder/Product.Seeder.csproj", "Product.Seeder/"]
RUN dotnet restore "Product.WebApi/Product.WebApi.csproj"
RUN dotnet restore "Product.Seeder/Product.Seeder.csproj"
COPY . .
WORKDIR "/src/Product.WebApi"
RUN dotnet build "Product.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Product.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build AS publish_seeder
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Product.Seeder.csproj" -c $BUILD_CONFIGURATION -o /app/seeder_publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.WebApi.dll"]
