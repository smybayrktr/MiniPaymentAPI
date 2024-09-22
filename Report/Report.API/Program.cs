using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.OpenApi.Models;
using Report.API.Middlewares;
using Report.Application.Handlers;
using Report.Application.Interfaces;
using Report.Application.Mapping;
using Report.Application.PipelineBehaviors;
using Report.Application.Services;
using Report.Application.Validators;
using Report.Contracts.DTOs;
using Report.Contracts.Queries;
using Report.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Register health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register MediatR
builder.Services.AddScoped(typeof(IRequestHandler<GetReportQuery, IEnumerable<TransactionReportDto>>), typeof(GetReportHandler));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<GetReportQueryValidator>();

// Register Validation Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Register PaymentServiceClient with HttpClient
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"]);
});

// Register ReportService
builder.Services.AddScoped<IReportService, ReportService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Report API",
        Version = "v1",
        Description = "#### Endpoints\n\n1. **Get Transactions Report**\n\n    - **URL:** `GET /api/report`\n    - **Description:** Retrieves a report of transactions based on provided search criteria.\n    - **Query Parameters:**\n        - `bankId` (string, optional): Filter by bank ID.\n        - `status` (string, optional): Filter by transaction status (`Success`, `Fail`).\n        - `orderReference` (string, optional): Filter by order reference.\n        - `startDate` (DateTime, optional): Start date for the report.\n        - `endDate` (DateTime, optional): End date for the report.\n    - **Responses:**\n        - `200 OK`: Returns a list of transactions matching the criteria.\n        - `400 Bad Request`: If the request parameters are invalid.\n        - `500 Internal Server Error`: If an error occurs while generating the report.",
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
