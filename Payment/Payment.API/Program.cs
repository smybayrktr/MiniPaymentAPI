using Payment.Infrastructure.Data;
using Payment.Infrastructure.Repositories;
using Payment.Application.Interfaces;
using Payment.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Payment.Application.Mapping;
using MediatR;
using Payment.Contracts.Commands;
using Payment.Domain.Entities;
using Payment.Application.Handlers;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;

var builder = WebApplication.CreateBuilder(args);


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

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register MediatR
builder.Services.AddScoped(typeof(IRequestHandler<PayTransactionCommand, Transaction>), typeof(PayTransactionHandler));
builder.Services.AddScoped(typeof(IRequestHandler<CancelTransactionCommand, Transaction>), typeof(CancelTransactionHandler));
builder.Services.AddScoped(typeof(IRequestHandler<RefundTransactionCommand, Transaction>), typeof(RefundTransactionHandler));
builder.Services.AddScoped(typeof(IRequestHandler<SearchPaymentQuery, IEnumerable<TransactionReportDto>>), typeof(SearchTransactionsHandler));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add Controllers
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

// Apply pending migrations and ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    dbContext.Database.Migrate();
}

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
