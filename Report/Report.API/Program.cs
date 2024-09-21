using System.Reflection;
using MediatR;
using Report.Application.Handlers;
using Report.Application.Interfaces;
using Report.Application.Mapping;
using Report.Application.Services;
using Report.Contracts.Queries;
using Report.Domain.Entities;
using Report.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

// Register PaymentServiceClient with HttpClient
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"]);
});

// Register ReportService
builder.Services.AddScoped<IReportService, ReportService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register MediatR
builder.Services.AddScoped(typeof(IRequestHandler<GetReportQuery, IEnumerable<Transaction>>), typeof(GetReportHandler));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
