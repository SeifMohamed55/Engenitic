# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image used when running from VS in fast mode
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# SDK image used to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["GraduationProject.csproj", "./"]
RUN dotnet restore

# Copy everything and publish the app
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GraduationProject.dll"]
