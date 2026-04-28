# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file first
COPY ShelfLife_backend_API.sln .

# Copy each project's .csproj file — names must exactly match your folder names
COPY ShelfLife_backend_API/ShelfLife_backend_API.csproj   ShelfLife_backend_API/
COPY Business_Layer/Business_Layer.csproj                 Business_Layer/
COPY Data_Layer/Data_Layer.csproj                         Data_Layer/
COPY Common_Class/Common_Class.csproj                     Common_Class/

# Restore NuGet packages (this layer is cached unless .csproj files change)
RUN dotnet restore

# Copy the rest of the source code
COPY ShellLife_backend_API/   ShellLife_backend_API/
COPY Business_Layer/          Business_Layer/
COPY Data_Layer/              Data_Layer/
COPY Common_Class/            Common_Class/

# Build and publish the API project in Release mode
WORKDIR /src/ShelfLife_backend_API
RUN dotnet publish -c Release -o /app/publish

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cloud Run requires port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShelfLife_backend_API.dll"]