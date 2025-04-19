using Infrastructure;
using MessageAggregator.Application.Interfaces;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Infrastructure;
using Microsoft.EntityFrameworkCore;
// using MessageAggregator.Domain.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddDbContext<AppDbContext>( options =>
            options.UseNpgsql( builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );
        builder.Services.AddScoped<ICategoryRepository, Infrastructure.Repositories.CategoryRepository>();
        builder.Services.AddScoped<ICategoryService, MessageAggregator.Application.Services.CategoryService>();
        builder.Services.AddScoped<IAIService, AiService>();
        builder.Services.AddScoped<IDcaService, DcaService>();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();