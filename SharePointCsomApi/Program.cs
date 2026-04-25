using SharePointCsomApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Services ANTES do Build
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddScoped<ISharePointContextFactory, SharePointContextFactory>();
builder.Services.AddScoped<ISharePointService, SharePointService>();
var app = builder.Build();

// Pipeline DEPOIS do Build
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}


app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
