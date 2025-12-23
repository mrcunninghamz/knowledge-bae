using KnowledgeBae.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBae.Data;

public class KnowledgeBaeDbContext : DbContext
{
    public KnowledgeBaeDbContext(DbContextOptions<KnowledgeBaeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Chunk> Chunks => Set<Chunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Chunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Text).HasMaxLength(10000);
            entity.Property(e => e.ReferenceDescription).HasMaxLength(500);
            entity.Property(e => e.ReferenceLink).HasMaxLength(2000);
            entity.Property(e => e.TextEmbedding).HasColumnType("vector(1536)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => e.TextEmbedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
        });
    }
}
