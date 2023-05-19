using Microsoft.Azure.Cosmos;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCosmosDb;
using Microsoft.SemanticKernel.Memory;

var cosmosMemoryStore = await CosmosMemoryStore.CreateAsync(new CosmosClient(
    "<Place the connection string here...>"),
    "power-reference-db");

var kernel = Kernel.Builder
    .Configure(c =>
    {
        c.AddAzureTextCompletionService(
            "davinci03",          // Azure OpenAI Deployment Name
            "<Place the Azure OpenAI endpoint here...>", // Azure OpenAI Endpoint
            "<Place the Azure OpenAI key here...>");
        c.AddAzureTextEmbeddingGenerationService(
            "embedding_ada02",          // Azure OpenAI Deployment Name
            "<Place the Azure OpenAI endpoint here...>", // Azure OpenAI Endpoint
            "<Place the Azure OpenAI key here...>");
    })
    .WithMemoryStorage(cosmosMemoryStore).Build();

const string MemoryCollectionName = "PowerDeviceMemory";

Func<string, Task> Search = async (string input) => {

    var memories = kernel.Memory.SearchAsync(MemoryCollectionName, input, limit: 5, minRelevanceScore: 0.77);

    var i = 0;
    await foreach (MemoryQueryResult memory in memories)
    {
        Console.WriteLine($"\nResult {++i}:");
        Console.WriteLine("  Device ID      : " + memory.Metadata.Id);
        Console.WriteLine("  Device Metadata: " + memory.Metadata.Text);
        Console.WriteLine("  Relevance      : " + memory.Relevance);
        Console.WriteLine("----------------------------------------------------------------------------\n");
    }
    if(i == 0)
    {
        Console.WriteLine($"\n:( Sorry, no matches found for '{input}'");
    }

    Console.WriteLine("\n");
};

Console.WriteLine(
    "=============================================================================================\n\n" +
    "Welcome to CE Power Device Search Engine!\n\n" +
    "WARNING: Only a few datacenters available in this demo. More will be coming soon!\n\n" +
    "=============================================================================================\n\n\n");

while (true)
{
    Console.Write("Search: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }

    await Search(userInput);
}