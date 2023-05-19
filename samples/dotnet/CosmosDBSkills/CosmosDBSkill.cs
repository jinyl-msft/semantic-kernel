// Copyright (c) Microsoft. All rights reserved.

namespace CosmosDBSkills
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Azure.Cosmos;

    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Orchestration;
    using Microsoft.SemanticKernel.SkillDefinition;

    using Newtonsoft.Json;

    /// <summary>
    /// Skill for interacting with Azure Cosmos DB.
    /// </summary>
    public class CosmosDBSkill
    {
        /// <summary>
        /// Name of the container which will be loaded
        /// </summary>
        public const string ContainerParamName = "container";

        /// <summary>
        /// SQL query to match against the data in the Cosmos DB container
        /// </summary>
        public const string QueryParamName = "query";

        /// <summary>
        /// Container path
        /// </summary>
        public const string ContainerPathParamName = "containerPath";

        /// <summary>
        /// Name of the memory collection used to store the code summaries.
        /// </summary>
        public const string MemoryCollectionNameParamName = "memoryCollectionName";

        private readonly IKernel _kernel;
        private readonly CosmosClient _sourceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBSkill"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="sourceClient">The source client.</param>
        public CosmosDBSkill(IKernel kernel, CosmosClient sourceClient)
        {
            this._kernel = kernel;
            this._sourceClient = sourceClient;
        }

        /// <summary>
        /// Summarize the code downloaded from the specified URI.
        /// </summary>
        /// <param name="source">URI to download the repository content to be summarized</param>
        /// <param name="context">Semantic kernel context</param>
        /// <returns>Task</returns>
        [SKFunction("Load a collection of documents from Azure Cosmos DB")]
        [SKFunctionName("LoadDocuments")]
        [SKFunctionInput(Description = "Database name of Azure Cosmos DB data to load")]
        [SKFunctionContextParameter(Name = ContainerParamName, Description = "Name of the container which will be loaded")]
        [SKFunctionContextParameter(Name = QueryParamName, Description = "SQL query to match against the data in the Cosmos DB container")]
        public async Task LoadDocumentsAsync(string source, SKContext context)
        {
            if (!context.Variables.Get(ContainerParamName, out string container) || string.IsNullOrEmpty(container))
            {
                container = "PowerDevice";
            }

            if (!context.Variables.Get(QueryParamName, out string query) || string.IsNullOrEmpty(query))
            {
                query = "SELECT * FROM c";
            }

            if (!context.Variables.Get(MemoryCollectionNameParamName, out string memCollection) || string.IsNullOrEmpty(memCollection))
            {
                memCollection = "PowerDeviceMemory";
            }

            string containerPath = Path.Combine(source, container);

            var database = source.Trim(new char[] { ' ', '/' });
            var context1 = new SKContext(logger: context.Log);
            context1.Variables.Set(ContainerPathParamName, containerPath);

            await this.ReadDocumentsAsync(database, container, query, memCollection);

            context.Variables.Set(MemoryCollectionNameParamName, memCollection);
        }

        /// <summary>
        /// Reads Json document into embeddings
        /// </summary>
        private async Task ReadDocumentsAsync(string databaseName, string containerName, string query, string memCollection)
        {
            var container = this._sourceClient.GetContainer(databaseName, containerName);

            var queryDef = new QueryDefinition(query);

            using (var iterator = container.GetItemQueryIterator<dynamic>(queryDef))
            {
                while (iterator.HasMoreResults)
                {
                    var items = await iterator.ReadNextAsync().ConfigureAwait(false);

                    foreach (var item in items)
                    {
                        await this._kernel.Memory.SaveInformationAsync(
                                memCollection,
                                JsonConvert.SerializeObject(item),
                                item.id?.ToString());
                    }
                }
            }
        }
    }
}