using GameOfLife.API.Data;
using GameOfLife.API.Models;
using System.Text.Json.Serialization;
using GameOfLife.API.Repositories;
using GameOfLife.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new Bool2DArrayJsonConverter());
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework
builder.Services.AddDbContext<GameOfLifeDbContext>(options =>
{
    // Prefer SQLite when configured
    var sqlite = builder.Configuration.GetConnectionString("SqliteConnection");
    if (!string.IsNullOrWhiteSpace(sqlite))
    {
        options.UseSqlite(sqlite);
        return;
    }

    // Then SQL Server when configured
    var sqlServer = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(sqlServer))
    {
        options.UseSqlServer(sqlServer);
        return;
    }

    // Fallback: use in-memory database (primarily for local dev/tests)
    options.UseInMemoryDatabase("GameOfLifeDb");
});

// Register repositories
builder.Services.AddScoped<IBoardStateRepository, BoardStateRepository>();

// Register validation services
builder.Services.AddScoped<IBoardValidationService, BoardValidationService>();

// Register Game of Life services
builder.Services.AddScoped<IGameOfLifeService, GameOfLifeService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    // Ensure database is created only when using in-memory provider
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<GameOfLifeDbContext>();
        var providerName = context.Database.ProviderName ?? string.Empty;
        if (providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            context.Database.EnsureCreated();
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
