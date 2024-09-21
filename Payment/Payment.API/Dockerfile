# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file
COPY ./MiniPaymentAPI.sln ./

# Copy project files
COPY ./Payment/Payment.API/Payment.API.csproj ./Payment/Payment.API/
COPY ./Payment/Payment.Application/Payment.Application.csproj ./Payment/Payment.Application/
COPY ./Payment/Payment.Contracts/Payment.Contracts.csproj ./Payment/Payment.Contracts/
COPY ./Payment/Payment.Domain/Payment.Domain.csproj ./Payment/Payment.Domain/
COPY ./Payment/Payment.Infrastructure/Payment.Infrastructure.csproj ./Payment/Payment.Infrastructure/

# Restore dependencies
RUN dotnet restore "./Payment/Payment.API/Payment.API.csproj"

# Copy the remaining source code
COPY ./Payment/Payment.API/. ./Payment/Payment.API/
COPY ./Payment/Payment.Application/. ./Payment/Payment.Application/
COPY ./Payment/Payment.Contracts/. ./Payment/Payment.Contracts/
COPY ./Payment/Payment.Domain/. ./Payment/Payment.Domain/
COPY ./Payment/Payment.Infrastructure/. ./Payment/Payment.Infrastructure/

# Build and publish the application
RUN dotnet publish "./Payment/Payment.API/Payment.API.csproj" -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish ./

# Expose the port that the application listens on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "Payment.API.dll"]
