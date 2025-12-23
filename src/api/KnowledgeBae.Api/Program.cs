using Microsoft.SemanticKernel;
using KnowledgeBae.Api.Configuration;
using KnowledgeBae.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add appsettings.local.json configuration
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Bind configuration
var appConfig = new AppConfiguration();
builder.Configuration.Bind(appConfig);
builder.Services.AddSingleton(appConfig);

// Add Semantic Kernel with Azure OpenAI
var kernelBuilder = Kernel.CreateBuilder();
if (!string.IsNullOrEmpty(appConfig.AzureOpenAI?.Endpoint) && 
    !string.IsNullOrEmpty(appConfig.AzureOpenAI?.ApiKey))
{
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: appConfig.AzureOpenAI.DeploymentName,
        endpoint: appConfig.AzureOpenAI.Endpoint,
        apiKey: appConfig.AzureOpenAI.ApiKey);
}
var kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);

// Register services
builder.Services.AddSingleton<SemanticSearchService>();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Hello World endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new { message = "Hello World from Knowledge Bae API!" });
})
.WithName("HelloWorld");

// Semantic search endpoint
app.MapGet("/search", async (string query, SemanticSearchService searchService) =>
{
    var results = await searchService.SearchAsync(query);
    return Results.Ok(new { query, results });
})
.WithName("SemanticSearch");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
