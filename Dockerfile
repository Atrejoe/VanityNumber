# Use the official .NET 10 RC SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files
COPY ["VanityNumberApi/VanityNumberApi.csproj", "VanityNumberApi/"]
COPY ["VanityNumberApi.Core/VanityNumberApi.Core.csproj", "VanityNumberApi.Core/"]

# Restore dependencies
RUN dotnet restore "VanityNumberApi/VanityNumberApi.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/VanityNumberApi"
RUN dotnet build "VanityNumberApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "VanityNumberApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port 8080 (default for ASP.NET Core in containers)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "VanityNumberApi.dll"]
