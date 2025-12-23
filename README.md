# knowledge-bae

This is a tool for AI agents like GitHub Copilot and Claude to be able to ingest documentation from a local git repository. Initially for simplicity, everything will be run locally: web api and mcp server.

## Project Structure

- **src/api/KnowledgeBae.Api** - .NET C# Web API with Semantic Kernel and Azure OpenAI integration
- **src/mcp** - Rust MCP (Model Context Protocol) server for AI agent integration

## Components

### Web API (src/api/KnowledgeBae.Api)

A .NET Web API project that provides:
- **Semantic Kernel Integration** - For AI-powered text processing
- **Azure OpenAI Client** - For accessing OpenAI models via Azure
- **pgvector Collection** - For semantic search capabilities
- **Configuration** - Loads settings from `appsettings.local.json`

#### Features
- Hello World endpoint at `/`
- Semantic search endpoint at `/search?query=<query>`

#### Configuration

Create an `appsettings.local.json` file in `src/api/KnowledgeBae.Api/` with your Azure OpenAI and PostgreSQL settings:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4"
  },
  "PostgreSQL": {
    "ConnectionString": "Host=localhost;Database=knowledge_bae;Username=postgres;Password=password"
  },
  "SemanticSearch": {
    "VectorDimensions": 1536,
    "CollectionName": "documents"
  }
}
```

#### Running the API

```bash
cd src/api/KnowledgeBae.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or the port specified in the output).

### MCP Server (src/mcp)

A Rust-based MCP server that provides:
- **Tools** - A `hello_world` tool that returns a greeting
- **Prompts** - A `hello_username` prompt that creates a greeting template for a given username

#### Running the MCP Server

```bash
cd src/mcp
cargo run
```

The MCP server communicates via stdio and is designed to be used by AI agents that support the Model Context Protocol.

## Development

### Prerequisites

- .NET 10.0 SDK or later
- Rust 1.70 or later
- PostgreSQL (optional, for full semantic search functionality)
- Azure OpenAI account (optional, for AI features)

### Building

Build the Web API:
```bash
cd src/api/KnowledgeBae.Api
dotnet build
```

Build the MCP Server:
```bash
cd src/mcp
cargo build
```

## License

This project is open source.
