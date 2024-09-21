using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration from ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
