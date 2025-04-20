using Infrastructure;
using MessageAggregator.Application.Service; // Keep for AiService, DcaService
// Removed using Infrastructure.Repositories;

// Removed using MessageAggregator.Application.Services;
using MessageAggregator.Domain.Interfaces; // Keep for IAIService, IDcaService
using MessageAggregator.Infrastructure; // Keep for AppDbContext
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));
builder.Services.AddHttpClient<IAiService, AiService>();
builder.Services.AddScoped<IDcaService, DcaService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем CORS-политику для фронта
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Включаем CORS до авторизации
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
