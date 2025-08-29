using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameOfLife.API.Data;

public class GameOfLifeDbContextFactory : IDesignTimeDbContextFactory<GameOfLifeDbContext>
{
    public GameOfLifeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameOfLifeDbContext>();

        // Prefer SQLite for design-time when provided
        var sqlite = Environment.GetEnvironmentVariable("ConnectionStrings__SqliteConnection");
        if (!string.IsNullOrWhiteSpace(sqlite))
        {
            optionsBuilder.UseSqlite(sqlite);
            return new GameOfLifeDbContext(optionsBuilder.Options);
        }

        // Fallback to SQL Server if provided
        var sqlServer = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(sqlServer))
        {
            optionsBuilder.UseSqlServer(sqlServer);
            return new GameOfLifeDbContext(optionsBuilder.Options);
        }

        // Final fallback: in-memory for design-time operations
        optionsBuilder.UseInMemoryDatabase("GameOfLifeDb");

        return new GameOfLifeDbContext(optionsBuilder.Options);
    }
}


