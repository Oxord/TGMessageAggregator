using Infrastructure; // Keep for AiService, DcaService
// Removed using Infrastructure.Repositories;
using MessageAggregator.Application.Service; // Keep for TelegramService, TelegramSettings
// Removed using MessageAggregator.Application.Services;
using MessageAggregator.Domain.Interfaces; // Keep for IAIService, IDcaService
using MessageAggregator.Domain.Models; // Added for User class
using MessageAggregator.Infrastructure; // Keep for AppDbContext
using Microsoft.AspNetCore.Identity; // Added for Identity services
using Microsoft.EntityFrameworkCore;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// --- Add CORS Services ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontend",
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:3000") // Allow the frontend origin
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // IMPORTANT: Allow credentials (cookies)
                      });
});

builder.Services.AddControllers();
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Add Identity Services ---
builder.Services.AddIdentity<User, IdentityRole>(options => {
    // Configure Identity options if needed (e.g., password requirements)
    options.SignIn.RequireConfirmedAccount = false; // Example: Disable email confirmation for simplicity
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); // Needed for things like password reset tokens

// --- Add Session Services ---
builder.Services.AddDistributedMemoryCache(); // Adds default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});


builder.Services.AddScoped<TelegramService>(); // Keep this, but it needs refactoring later
// Removed CategoryRepository registration
// Removed CategoryService registration
builder.Services.AddHttpClient<IAIService, AiService>(); // Changed from AddScoped to AddHttpClient
builder.Services.AddScoped<IDcaService, DcaService>();

// --- Add Razor Pages for Identity UI ---
builder.Services.AddRazorPages(); // Add services for Razor Pages

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Removed duplicate AddEndpointsApiExplorer/AddSwaggerGen
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Use CORS Middleware ---
// IMPORTANT: Must be called BEFORE UseRouting, UseAuthentication, UseAuthorization
app.UseCors("AllowFrontend"); // Apply the policy we defined

// --- Add Session Middleware ---
// IMPORTANT: Must be called before UseAuthentication and UseAuthorization
app.UseSession();

app.UseAuthorization();


app.MapControllers();
app.MapRazorPages(); // Map endpoints for Razor Pages (including Identity UI)

app.Run();
