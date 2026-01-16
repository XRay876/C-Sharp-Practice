using StockWatcher.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace StockWatcher.Api.Data;

public class AppDbContext : DbContext
{
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настраиваем точность decimal (18 знаков всего, 2 после запятой)
        // modelBuilder.Entity<Stock>()
        //     .Property(s => s.Price)
        //     .HasPrecision(18, 2);

        // modelBuilder.Entity<Stock>()
        //     .HasIndex(s => s.Ticker)
        //     .IsUnique();
        modelBuilder.Entity<Stock>(entity => 
        {
            // Настройка индекса
            entity.HasIndex(s => s.Ticker)
                .IsUnique();

            // Настройка точности цены
            entity.Property(s => s.Price)
                .HasPrecision(18, 2);
                
            // Можно сразу ограничить длину строки, чтобы не было NVARCHAR(MAX)
            entity.Property(s => s.Ticker)
                .HasMaxLength(10)
                .IsRequired();
        });

        // Можно явно указать связь, если EF не догадался
        modelBuilder.Entity<Stock>()
            .HasMany(s => s.PriceHistory)
            .WithOne(h => h.Stock)
            .HasForeignKey(h => h.StockId)
            .OnDelete(DeleteBehavior.Cascade); // Если удалим акцию, удалится и её история
    }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Stock> Stocks => Set<Stock>();


}