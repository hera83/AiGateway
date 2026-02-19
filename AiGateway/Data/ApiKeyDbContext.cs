using Microsoft.EntityFrameworkCore;

namespace AiGateway.Data;

/// <summary>
/// EF Core DbContext for API key storage.
/// </summary>
public class ApiKeyDbContext : DbContext
{
    public DbSet<ClientKeyEntity> ClientKeys { get; set; } = null!;

    public ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClientKeyEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AppName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AppContact).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AppNote).HasMaxLength(1000);
            entity.Property(e => e.KeyHash).IsRequired();
            entity.Property(e => e.Salt).IsRequired();
        });
    }
}
