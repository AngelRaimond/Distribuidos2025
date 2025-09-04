using Microsoft.EntityFrameworkCore;
using PlantApi.Infrastructure.Entities;


namespace PlantApi.Infrastructure;

public class RelationalDbContext : DbContext
{
    public DbSet<PlantEntity> Plants { get; set; }
    public RelationalDbContext(DbContextOptions<RelationalDbContext> db) : base(db)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<PlantEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ScientificName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Family).IsRequired();
            entity.Property(e => e.MaxHeight).IsRequired();
            entity.Property(e => e.MaxAge).IsRequired();
            entity.Property(e => e.ConservationLevel).IsRequired();
        });
    }
}

