using Microsoft.Azure.Cosmos;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCosmosDb;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Orchestration;

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


// Alternative using OpenAI
// kernel.Config.AddOpenAITextCompletionService("davinci-openai",
//     "text-davinci-003",               // OpenAI Model name
//     "...your OpenAI API Key..."       // OpenAI API Key
// );

const string MemoryCollectionName = "PowerDeviceMemory";

kernel.ImportSkill(new TextMemorySkill());

SKContext context = kernel.CreateNewContext();

context[TextMemorySkill.CollectionParam] = MemoryCollectionName;
context[TextMemorySkill.RelevanceParam] = "0.77";

var summarizeSkill = kernel.ImportSemanticSkillFromDirectory("C:\\Users\\jinyl\\Source\\Repos\\AzureOpenAI\\src\\Skills", "SummarizeSkill");
var qaSkill = kernel.ImportSemanticSkillFromDirectory("C:\\Users\\jinyl\\Source\\Repos\\AzureOpenAI\\src\\Skills", "QASkill");

var history = "";
context["history"] = history;
context["object"] = "power devices";

Func<string, Task> Chat = async (string input) => {
    // Save new message in the context variables
    context["userInput"] = input;

    context = await summarizeSkill["Question"].InvokeAsync(context);
    
    // Process the user message and get an answer
    var answer = await qaSkill["CEHierarchyMemoryQuery"].InvokeAsync(context);

    // Append the new interaction to the chat history
    history += $"\nUser: {input}\nChat Agent: {answer}\n"; context["history"] = history;

    // Show the bot response
    Console.WriteLine("Chat Agent: " + context.Result);
};

Console.WriteLine(
    "=============================================================================================\n\n" +
    "Hi, there!\n" +
    "I'm a chat agent to help you on any questions for CE power devices.\n" +
    "You can type your questions below or 'exit' to end chat.\n\n" +
    "WARNING: Only a few datacenters available in this demo. More will be coming soon!\n\n" +
    "=============================================================================================\n\n");

while(true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    await Chat(userInput);
}
