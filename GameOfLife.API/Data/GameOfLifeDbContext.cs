using GameOfLife.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.API.Data;

public class GameOfLifeDbContext : DbContext
{
    public GameOfLifeDbContext(DbContextOptions<GameOfLifeDbContext> options) : base(options)
    {
    }

    public DbSet<BoardState> BoardStates { get; set; }
    public DbSet<BoardStateHistory> BoardStateHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BoardState entity
        modelBuilder.Entity<BoardState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GridData).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.Rows).IsRequired();
            entity.Property(e => e.Columns).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Generation).IsRequired();
            entity.Property(e => e.LastModifiedAt).IsRequired();
            
            // Add indexes for better performance
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.LastModifiedAt);
        });

        // Configure BoardStateHistory entity
        modelBuilder.Entity<BoardStateHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BoardStateId).IsRequired();
            entity.Property(e => e.GridData).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.Generation).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Configure relationship
            entity.HasOne(e => e.BoardState)
                  .WithMany(e => e.History)
                  .HasForeignKey(e => e.BoardStateId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Add indexes
            entity.HasIndex(e => e.BoardStateId);
            entity.HasIndex(e => e.Generation);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
