using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KnowledgeBae.Data;

public class KnowledgeBaeDbContextFactory : IDesignTimeDbContextFactory<KnowledgeBaeDbContext>
{
    public KnowledgeBaeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KnowledgeBaeDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("KNOWLEDGE_BAE_CONNECTION_STRING") 
            ?? "Host=localhost;Database=knowledgebae;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

        return new KnowledgeBaeDbContext(optionsBuilder.Options);
    }
}
