# Use the official .NET 10 RC SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

# Copy solution file
COPY VanityNumber.sln ./

# Copy project files for restore (improves Docker layer caching)
COPY VanityNumber.Api/VanityNumber.Api.csproj VanityNumber.Api/
COPY VanityNumber.Core/VanityNumber.Core.csproj VanityNumber.Core/

# Restore dependencies
RUN dotnet restore VanityNumber.Api/VanityNumber.Api.csproj

# Copy all source files
COPY VanityNumber.Api/ VanityNumber.Api/
COPY VanityNumber.Core/ VanityNumber.Core/

# Build the application
WORKDIR /src/VanityNumber.Api
RUN dotnet build VanityNumber.Api.csproj -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish VanityNumber.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port 8080 (default for ASP.NET Core in containers)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "VanityNumber.Api.dll"]
