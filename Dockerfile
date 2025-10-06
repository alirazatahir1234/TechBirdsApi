# Use the official .NET 9 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Set the working directory
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Expose the port
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development

# Run the application
ENTRYPOINT ["dotnet", "TechBirdsApi.dll"]