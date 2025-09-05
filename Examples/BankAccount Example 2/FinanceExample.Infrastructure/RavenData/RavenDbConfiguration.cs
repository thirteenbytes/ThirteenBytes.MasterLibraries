using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Raven.Embedded;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FinanceExample.Infrastructure.RavenData.Converters;

namespace FinanceExample.Infrastructure.RavenData
{
    public static class RavenDbConfiguration
    {
        public static IDocumentStore CreateEmbeddedStore(IConfiguration configuration, ILogger? logger = null)
        {
            var dataDirectory = configuration.GetConnectionString("RavenDataDirectory") ?? "Data\\RavenDB";
            var databaseName = configuration.GetConnectionString("RavenDatabaseName") ?? "FinanceDB";

            logger?.LogInformation("Initializing embedded RavenDB with data directory: {DataDirectory}", dataDirectory);

            // Start the embedded server
            EmbeddedServer.Instance.StartServer(new ServerOptions
            {
                DataDirectory = dataDirectory,
                ServerUrl = "http://127.0.0.1:0" // Use random available port
            });

            // Create document store
            var store = EmbeddedServer.Instance.GetDocumentStore(new DatabaseOptions(databaseName)
            {
                Conventions = new DocumentConventions
                {
                    // Configure JSON serialization using the Newtonsoft.Json serializer
                    Serialization = new NewtonsoftJsonSerializationConventions
                    {
                        CustomizeJsonSerializer = serializer =>
                        {
                            serializer.TypeNameHandling = TypeNameHandling.Auto;
                            serializer.NullValueHandling = NullValueHandling.Ignore;
                            // Add custom converter for Money
                            serializer.Converters.Add(new MoneyJsonConverter());
                        }
                    }
                }
            });
            
            logger?.LogInformation("RavenDB embedded store initialized for database: {DatabaseName}", databaseName);

            return store;
        }
    }
}