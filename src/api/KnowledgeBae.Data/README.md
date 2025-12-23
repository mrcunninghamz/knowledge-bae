# KnowledgeBae.Data - Entity Framework Core Migrations

This project contains Entity Framework Core data access layer with PostgreSQL and pgvector support.

## Prerequisites

- PostgreSQL with pgvector extension installed
- .NET 10.0 SDK
- dotnet-ef tool (installed globally)

## Setup PostgreSQL with pgvector

```bash
# Connect to your PostgreSQL database
psql -U postgres

# Create the database
CREATE DATABASE knowledgebae;

# Connect to the database
\c knowledgebae

# Enable the pgvector extension
CREATE EXTENSION vector;
```

## Connection String

Set the connection string as an environment variable:

```bash
export KNOWLEDGE_BAE_CONNECTION_STRING="Host=localhost;Database=knowledgebae;Username=postgres;Password=postgres"
```

Or add to your `~/.zshenv`:

```bash
export KNOWLEDGE_BAE_CONNECTION_STRING="Host=localhost;Database=knowledgebae;Username=postgres;Password=postgres"
```

If not set, the default connection string will be used: `Host=localhost;Database=knowledgebae;Username=postgres;Password=postgres`

## Migration Commands

All commands should be run from the `src/api/KnowledgeBae.Data` directory.

### Create a New Migration

```bash
cd src/api/KnowledgeBae.Data
dotnet ef migrations add <MigrationName>
```

Example:
```bash
dotnet ef migrations add AddChunkIndexes
```

### Apply Migrations to Database

```bash
dotnet ef database update
```

### Revert to a Specific Migration

```bash
dotnet ef database update <MigrationName>
```

Example to revert to initial state:
```bash
dotnet ef database update InitialCreate
```

To revert all migrations:
```bash
dotnet ef database update 0
```

### Remove the Last Migration

**Note:** This only removes the migration files. If the migration has been applied to the database, you must revert it first.

```bash
# First, revert the database to the previous migration
dotnet ef database update <PreviousMigrationName>

# Then remove the migration files
dotnet ef migrations remove
```

### List All Migrations

```bash
dotnet ef migrations list
```

### Generate SQL Script

To generate a SQL script without applying changes:

```bash
# Generate script for all migrations
dotnet ef migrations script

# Generate script from specific migration to another
dotnet ef migrations script <FromMigration> <ToMigration>

# Generate script and save to file
dotnet ef migrations script -o migration.sql
```

## Project Structure

```
KnowledgeBae.Data/
├── Entities/
│   └── Chunk.cs                          # Entity model
├── Migrations/                           # Generated migration files
├── KnowledgeBaeDbContext.cs             # DbContext with pgvector configuration
├── KnowledgeBaeDbContextFactory.cs      # Design-time factory for migrations
└── README.md                            # This file
```

## Entity: Chunk

The `Chunk` entity stores text snippets with vector embeddings for semantic search:

- `Id` (Guid): Primary key
- `Text` (string): The text content
- `ReferenceDescription` (string): Optional description
- `ReferenceLink` (string): Optional reference URL
- `TextEmbedding` (Vector): 1536-dimensional vector embedding
- `CreatedAt` (DateTime): Timestamp
- `UpdatedAt` (DateTime): Timestamp

The `TextEmbedding` column uses pgvector's HNSW index for efficient similarity search.

## Troubleshooting

### "Could not execute because the specified command or file was not found"

Install the EF Core tools globally:
```bash
dotnet tool install --global dotnet-ef
```

### "No DbContext was found"

Make sure you're running commands from the `KnowledgeBae.Data` project directory.

### Connection errors

Verify your connection string and ensure PostgreSQL is running:
```bash
psql -U postgres -h localhost
```
