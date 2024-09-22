# Mini Payment API

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technologies Used](#technologies-used)
3. [API Specifications](#api-specifications)
    - [Payment API](#payment-api)
    - [Report API](#report-api)
    - [API Gateway](#api-gateway)
4. [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Running the Application](#running-the-application)
    - [Running the Application With Docker Compose](#running-the-application-with-docker-compose)

---

## Project Overview

This project comprises a suite of microservices designed to handle payment processing, reporting, and routing through an
API Gateway. The architecture follows best practices for microservices, ensuring scalability, maintainability, and
robustness.

- **Payment API**: Manages transactions, including payment processing, cancellation, and refunds.
- **Report API**: Generates reports based on transaction data, providing insights and analytics.
- **API Gateway**: Acts as a single entry point for all client requests, routing them to the appropriate microservice.

## Technologies Used

- **.NET 8.0**: Framework for building APIs and services.
- **MediatR**: Implements the Mediator pattern for handling commands and queries.
- **AutoMapper**: Simplifies object-object mapping.
- **Entity Framework Core**: ORM for data access.
- **xUnit**: Testing framework.
- **Moq**: Mocking library for unit tests.
- **FluentAssertions**: Provides more readable and fluent assertion syntax.
- **Swagger/OpenAPI**: API documentation and testing interface.
- **Docker**: Containerization of services (optional).
- **API Gateway**: Ocelot for routing and aggregating API requests.

## API Specifications

### Payment API

**Base URL:** `/api/payment`

#### Endpoints

1. **Process Payment**

    - **URL:** `POST /api/payment/pay`
    - **Description:** Processes a new payment transaction.
    - **Request Body:**
        ```json
        {
            "bankId": "Akbank",
            "totalAmount": 1000.00,
            "orderReference": "ORDER_001"
        }
        ```
    - **Responses:**
        - `200 OK`: Returns the created transaction.
        - `400 Bad Request`: If the request parameters are invalid.
        - `500 Internal Server Error`: If an error occurs during processing.

2. **Cancel Transaction**

    - **URL:** `POST /api/payment/cancel/{transactionId}`
    - **Description:** Cancels an existing transaction.
    - **Parameters:**
        - `transactionId` (Guid): The ID of the transaction to cancel.
    - **Responses:**
        - `200 OK`: Returns the updated transaction.
        - `404 Not Found`: If the transaction does not exist.
        - `400 Bad Request`: If the transaction cannot be canceled (e.g., different day).
        - `500 Internal Server Error`: If an error occurs during processing.

3. **Refund Transaction**

    - **URL:** `POST /api/payment/refund/{transactionId}`
    - **Description:** Refunds an existing transaction.
    - **Parameters:**
        - `transactionId` (Guid): The ID of the transaction to refund.
    - **Responses:**
        - `200 OK`: Returns the updated transaction.
        - `404 Not Found`: If the transaction does not exist.
        - `400 Bad Request`: If the refund is not allowed (e.g., within one day).
        - `500 Internal Server Error`: If an error occurs during processing.

### Report API

**Base URL:** `/api/report`

#### Endpoints

1. **Get Transactions Report**

    - **URL:** `GET /api/report`
    - **Description:** Retrieves a report of transactions based on provided search criteria.
    - **Query Parameters:**
        - `bankId` (string, optional): Filter by bank ID.
        - `status` (string, optional): Filter by transaction status (`Success`, `Fail`).
        - `orderReference` (string, optional): Filter by order reference.
        - `startDate` (DateTime, optional): Start date for the report.
        - `endDate` (DateTime, optional): End date for the report.
    - **Responses:**
        - `200 OK`: Returns a list of transactions matching the criteria.
        - `400 Bad Request`: If the request parameters are invalid.
        - `500 Internal Server Error`: If an error occurs while generating the report.

### API Gateway

**Base URL:** `/`

#### Endpoints

1. **Payment API Routes**

    - **Path:** `/payment/*`
    - **Description:** Routes to the **Payment API**.
    - **Example:**
        - `POST /payment/pay` → **Payment API**

2. **Report API Routes**

    - **Path:** `/report/*`
    - **Description:** Routes to the **Report API**.
    - **Example:**
        - `GET /report` → **Report API**

## Getting Started

### Prerequisites

- **.NET 8.0 SDK** or later
- **PostgreSQL**
- **Docker** (optional, for containerization)

### Installation

1. **Clone the Repository:**
    ```bash
    git clone https://github.com/smybayrktr/MiniPaymentAPI.git
    cd MiniPaymentAPI
    ```

2. **Navigate to Each Project and Restore Dependencies:**
    ```bash
    cd API/Payment.API
    dotnet restore

    cd ../../Report/API/Report.API
    dotnet restore

    cd ../../../Application/Payment.Application
    dotnet restore

    cd ../../Infrastructure/Payment.Infrastructure
    dotnet restore

    # Repeat for other projects as necessary
    ```

3. **Configure Database Connections:**
    - Update the `appsettings.json` files in **Payment.API** with the correct database connection string.

### Running the Application

1. **Start API Services:**
    - **Payment API:**
        ```bash
        cd API/Payment.API
        dotnet run
        ```
    - **Report API:**
        ```bash
        cd API/Report.API
        dotnet run
        ```
    - **API Gateway:**
        ```bash
        cd API/ApiGateway
        dotnet run
        ```

2. **Access APIs:**
    - **Payment API:** `https://localhost:5001/api/payment`
    - **Report API:** `https://localhost:5002/api/report`
    - **API Gateway:** `https://localhost:5000/`

### Running the Application With Docker Compose

For a streamlined setup, you can use Docker Compose to containerize and run all services together. This approach ensures
that all dependencies are correctly orchestrated, simplifying deployment and scalability.

1. **Prerequisites:**

   Ensure Docker is installed on your machine. You can download it from
   the [Docker Official Website](https://www.docker.com/products/docker-desktop/).

2. **Build and Run the Application Stack:**

   From the root directory of your project (where the docker-compose.yml file is located), execute the following
   command:
     ```bash
        docker-compose up --build
     ```

3. **Access APIs:**
    - **Payment API:** `https://localhost:5001/api/payment`
    - **Report API:** `https://localhost:5002/api/report`
    - **API Gateway:** `https://localhost:5000/`
