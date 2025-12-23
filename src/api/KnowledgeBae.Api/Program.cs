using System.ClientModel.Primitives;
using System.Net;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.SemanticKernel;
using KnowledgeBae.Api.Configuration;
using KnowledgeBae.Api.Models;
using KnowledgeBae.Api.Services;
using KnowledgeBae.Data;
using Microsoft.Extensions.AI;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add appsettings.local.json configuration
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Bind configuration
var appConfig = new AppConfiguration();
builder.Configuration.Bind(appConfig);
builder.Services.AddSingleton(appConfig);

// Get the factory-created HttpClient for AzureOpenAIClient
builder.Services.AddHttpClient("AzureOpenAIClient")
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(10, Int32.MaxValue))
    .AddPolicyHandler((provider, _) => GetRetryOnRateLimitingPolicy(provider));
var httpClientFactory = builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
var azureOpenAiHttpClient = httpClientFactory.CreateClient("AzureOpenAIClient");

// Add Semantic Kernel with Azure OpenAI
var kernelBuilder = builder.Services.AddKernel();
if (!string.IsNullOrEmpty(appConfig.AzureOpenAI?.Endpoint) && 
    !string.IsNullOrEmpty(appConfig.AzureOpenAI?.ApiKey))
{
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: appConfig.AzureOpenAI.DeploymentName,
        endpoint: appConfig.AzureOpenAI.Endpoint,
        apiKey: appConfig.AzureOpenAI.ApiKey,
        httpClient: azureOpenAiHttpClient);
}
else
{
    throw new Exception($"The configuration {nameof(appConfig.AzureOpenAI)} is missing some values");
}

builder.Services.AddSingleton<IEmbeddingGenerator>(
    sp => new AzureOpenAIClient(
            new Uri(appConfig.AzureOpenAIEmbeddings.Endpoint), 
            new AzureCliCredential(), 
            options: new AzureOpenAIClientOptions { Transport = new HttpClientPipelineTransport(azureOpenAiHttpClient) })
        .GetEmbeddingClient(appConfig.AzureOpenAIEmbeddings.DeploymentName)
        .AsIEmbeddingGenerator());

kernelBuilder.Services.AddPostgresCollection<Guid, TextSnippet<Guid>>("Chunks",
    appConfig.PostgreSQL.ConnectionString); 
//TODO: Add a text search implementation that uses the registered vector store record collection for search.
//kernelBuilder.AddVectorStoreTextSearch<Chunk>(resultMapper:

// Register services
builder.Services.AddSingleton<SemanticSearchService>();


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


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetRetryOnRateLimitingPolicy(IServiceProvider provider)
{
    var logger = provider.GetService<ILogger<Program>>();
    // Retry when these status codes are encountered.
    HttpStatusCode[] httpStatusCodesWorthRetrying = {
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.GatewayTimeout // 504
    };
    // Define our waitAndRetry policy: retry n times with an exponential backoff in case the API throttles us for too many requests.
    var waitAndRetryPolicy = Policy
        .HandleResult<HttpResponseMessage> (e => e.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                    e.StatusCode == (System.Net.HttpStatusCode) 429)
        .WaitAndRetryAsync(10, // Retry 10 times with a delay between retries before ultimately giving up
            attempt => TimeSpan.FromSeconds(0.25 * Math.Pow(2, attempt)), // Back off!  2, 4, 8, 16 etc times 1/4-second
            //attempt => TimeSpan.FromSeconds(6), // Wait 6 seconds between retries
            (exception, calculatedWaitDuration) => {
                logger.LogInformation($"API server is throttling our requests. Automatically delaying for {calculatedWaitDuration.TotalMilliseconds}ms");
            }
        );

    // Define our first CircuitBreaker policy: Break if the action fails 4 times in a row.
    // a number of recoverable status messages, such as 500, 502, and 504.
    var circuitBreakerPolicyForRecoverable = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(3),
            onBreak: (outcome, breakDelay) => {
                logger.LogInformation($"Polly Circuit Breaker logging: Breaking the circuit for {breakDelay.TotalMilliseconds}ms due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
            },
            onReset: () => logger.LogInformation("Polly Circuit Breaker logging: Call ok... closed the circuit again"),
            onHalfOpen: () => logger.LogInformation("Polly Circuit Breaker logging: Half-open: Next call is a trial")
        );

    // Combine the waitAndRetryPolicy and circuit breaker policy into a PolicyWrap. This defines our resiliency strategy.
    return Policy.WrapAsync(waitAndRetryPolicy, circuitBreakerPolicyForRecoverable);
}