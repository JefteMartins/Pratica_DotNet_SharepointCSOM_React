using HotelAPI.Services;
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
app.UseCors("AllowFrontend"); // Ativa o CORS
app.MapControllers();

app.Run();
