using HotelAPI.Services;
using HotelAPI.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Porta padrão do Vite
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// SharePoint Infrastructure
builder.Services.AddScoped<ISharePointContextFactory, SharePointContextFactory>();

// Business Services
builder.Services.AddScoped<ISharePointService, SharePointService>();
builder.Services.AddScoped<ISharePointProvisioningService, SharePointProvisioningService>();
builder.Services.AddScoped<ISharePointSeedService, SharePointSeedService>();

// Lab Services
builder.Services.AddScoped<ILabService, LabService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
