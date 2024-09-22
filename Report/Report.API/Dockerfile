# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file
COPY ./MiniPaymentAPI.sln ./

# Copy project files
COPY ./Report/Report.API/Report.API.csproj ./Report/Report.API/
COPY ./Report/Report.Application/Report.Application.csproj ./Report/Report.Application/
COPY ./Report/Report.Contracts/Report.Contracts.csproj ./Report/Report.Contracts/
COPY ./Report/Report.Infrastructure/Report.Infrastructure.csproj ./Report/Report.Infrastructure/

# Restore dependencies
RUN dotnet restore "./Report/Report.API/Report.API.csproj"

# Copy the remaining source code
COPY ./Report/Report.API/. ./Report/Report.API/
COPY ./Report/Report.Application/. ./Report/Report.Application/
COPY ./Report/Report.Contracts/. ./Report/Report.Contracts/
COPY ./Report/Report.Infrastructure/. ./Report/Report.Infrastructure/

# Build and publish the application
RUN dotnet publish "./Report/Report.API/Report.API.csproj" -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish ./

# Expose the port that the application listens on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "Report.API.dll"]
