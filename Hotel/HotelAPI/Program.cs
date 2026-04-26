using HotelAPI.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// SharePoint Services
builder.Services.AddScoped<ISharePointContextFactory, SharePointContextFactory>();
builder.Services.AddScoped<ISharePointService, SharePointService>();
builder.Services.AddScoped<ISharePointProvisioningService, SharePointProvisioningService>();
builder.Services.AddScoped<ISharePointSeedService, SharePointSeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Adiciona o Scalar
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
