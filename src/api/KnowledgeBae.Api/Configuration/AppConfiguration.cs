namespace KnowledgeBae.Api.Configuration;

public class AppConfiguration
{
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();
    public AzureOpenAIEmbeddingsConfiguration  AzureOpenAIEmbeddings { get; set; } = new();
    public PostgreSQLConfiguration PostgreSQL { get; set; } = new();
    public SemanticSearchConfiguration SemanticSearch { get; set; } = new();
}

public class AzureOpenAIConfiguration
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
}

public class AzureOpenAIEmbeddingsConfiguration
{
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    
}

public class PostgreSQLConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class SemanticSearchConfiguration
{
    public int VectorDimensions { get; set; }
    public string CollectionName { get; set; } = string.Empty;
}
