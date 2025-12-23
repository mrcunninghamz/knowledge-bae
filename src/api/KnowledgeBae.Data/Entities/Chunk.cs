namespace KnowledgeBae.Data.Entities;

public class Chunk
{
    public Guid Id { get; set; }
    public string? Text { get; set; }
    public string? ReferenceDescription { get; set; }
    public string? ReferenceLink { get; set; }
    public Pgvector.Vector? TextEmbedding { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
