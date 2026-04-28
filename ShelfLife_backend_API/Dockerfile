# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShellLife_backend_API/ShelfLife_backend_API.csproj", "ShellLife_backend_API/"]
COPY ["Business_Layer/Business_Layer.csproj", "Business_Layer/"]
COPY ["Common_Class/Common_Class.csproj", "Common_Class/"]
COPY ["Data_Layer/Data_Layer.csproj", "Data_Layer/"]
RUN dotnet restore "./ShellLife_backend_API/ShelfLife_backend_API.csproj"
COPY . .
WORKDIR "/src/ShellLife_backend_API"
RUN dotnet build "./ShelfLife_backend_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShelfLife_backend_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShelfLife_backend_API.dll"]