using EzCatalog.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace EzCatalog.Infrastructure.EntityFramework;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductDbModel> Products => Set<ProductDbModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
