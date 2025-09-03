using Microsoft.EntityFrameworkCore;
using PokemonApi.Infrastructure.Entities;

namespace PokemonApi.Infrastructure;

public class RelationalDbContext : DbContext
{
    public DbSet<PokemonEntity> Pokemons { get; set; }
    public RelationalDbContext(DbContextOptions<RelationalDbContext> db) : base(db)
    {

<<<<<<< HEAD
=======
        public RelationalDbContext(DbContextOptions<RelationalDbContext> db) : base(db)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PokemonEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Level).IsRequired();
                entity.Property(e => e.Attack).IsRequired();
                entity.Property(e => e.Defense).IsRequired();
                entity.Property(e => e.Speed).IsRequired();
                entity.Property(e => e.Weight).IsRequired();
            });
        }
>>>>>>> 2797f66e41abaefedb00fba902626acf91ac23ff
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure your entities here

        modelBuilder.Entity<PokemonEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.Attack).IsRequired();
            entity.Property(e => e.Defense).IsRequired();
            entity.Property(e => e.Speed).IsRequired();
            entity.Property(e => e.Weight).IsRequired();
        });
    }
}