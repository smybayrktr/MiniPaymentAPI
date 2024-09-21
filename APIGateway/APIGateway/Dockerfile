# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file
COPY ./MiniPaymentAPI.sln ./

# Copy project files
COPY ./APIGateway/APIGateway/APIGateway.csproj ./APIGateway/APIGateway/

# Restore dependencies
RUN dotnet restore "./APIGateway/APIGateway/APIGateway.csproj"

# Copy the remaining source code
COPY ./APIGateway/APIGateway/. ./APIGateway/APIGateway/

# Copy ocelot.json configuration file
COPY ./APIGateway/APIGateway/ocelot.json ./APIGateway/APIGateway/

# Build and publish the application
RUN dotnet publish "./APIGateway/APIGateway/APIGateway.csproj" -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime 
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish ./

# Expose the port that the application listens on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "APIGateway.dll"]
