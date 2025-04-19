using Infrastructure; // Keep for AiService, DcaService
// Removed using Infrastructure.Repositories;
using MessageAggregator.Application.Service; // Keep for TelegramService, TelegramSettings
// Removed using MessageAggregator.Application.Services;
using MessageAggregator.Domain.Interfaces; // Keep for IAIService, IDcaService
using MessageAggregator.Infrastructure; // Keep for AppDbContext
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<TelegramService>();
// Removed CategoryRepository registration
// Removed CategoryService registration
builder.Services.AddHttpClient<IAIService, AiService>(); // Changed from AddScoped to AddHttpClient
builder.Services.AddScoped<IDcaService, DcaService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
