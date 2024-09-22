using Payment.Infrastructure.Data;
using Payment.Infrastructure.Repositories;
using Payment.Application.Interfaces;
using Payment.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using FluentValidation;
using Payment.Application.Mapping;
using MediatR;
using Microsoft.OpenApi.Models;
using Payment.API.Middlewares;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Application.Handlers;
using Payment.Application.PipelineBehaviors;
using Payment.Application.Validators;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();


// Register health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register MediatR
builder.Services.AddScoped(typeof(IRequestHandler<PayTransactionCommand, Transaction>), typeof(PayTransactionHandler));
builder.Services.AddScoped(typeof(IRequestHandler<CancelTransactionCommand, Transaction>),
    typeof(CancelTransactionHandler));
builder.Services.AddScoped(typeof(IRequestHandler<RefundTransactionCommand, Transaction>),
    typeof(RefundTransactionHandler));
builder.Services.AddScoped(typeof(IRequestHandler<SearchPaymentQuery, IEnumerable<TransactionReportDto>>),
    typeof(SearchPaymentHandler));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<SearchPaymentQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PayTransactionCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CancelTransactionCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RefundTransactionCommandValidator>();

// Register Validation Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Register DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PaymentDB")));

// Register repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionDetailRepository, TransactionDetailRepository>();

// Register bank services
builder.Services.AddScoped<AkbankService>();
builder.Services.AddScoped<GarantiService>();
builder.Services.AddScoped<YapiKrediService>();
builder.Services.AddScoped<IBankFactory, BankFactory>();

// Register application services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment API",
        Version = "v1",
        Description = "#### Endpoints\n\n1. **Process Payment**\n\n    - **URL:** `POST /api/payment/pay`\n    - **Description:** Processes a new payment transaction.\n    - **Request Body:**\n        ```json\n        {\n            \"bankId\": \"Akbank\",\n            \"totalAmount\": 1000.00,\n            \"orderReference\": \"ORDER_001\"\n        }\n        ```\n    - **Responses:**\n        - `200 OK`: Returns the created transaction.\n        - `400 Bad Request`: If the request parameters are invalid.\n        - `500 Internal Server Error`: If an error occurs during processing.\n\n2. **Cancel Transaction**\n\n    - **URL:** `POST /api/payment/cancel/{transactionId}`\n    - **Description:** Cancels an existing transaction.\n    - **Parameters:**\n        - `transactionId` (Guid): The ID of the transaction to cancel.\n    - **Responses:**\n        - `200 OK`: Returns the updated transaction.\n        - `400 Bad Request`: If the transaction cannot be canceled (e.g., different day).\n        - `500 Internal Server Error`: If an error occurs during processing.\n\n3. **Refund Transaction**\n\n    - **URL:** `POST /api/payment/refund/{transactionId}`\n    - **Description:** Refunds an existing transaction.\n    - **Parameters:**\n        - `transactionId` (Guid): The ID of the transaction to refund.\n    - **Responses:**\n        - `200 OK`: Returns the updated transaction.\n        - `400 Bad Request`: If the refund is not allowed (e.g., within one day).\n        - `500 Internal Server Error`: If an error occurs during processing.\n"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Apply pending migrations and ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1");
    });
}

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Name == "self"
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true
});

app.Run();