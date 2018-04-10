using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace NSMGFunc
{
    public static class FuncHttp
    {
        static private string CosmosWifiEndpointUrl = Environment.GetEnvironmentVariable("CosmosWifiEndpoint");
        static private string CosmosPrimaryKey = Environment.GetEnvironmentVariable("CosmosWifiPrimaryKey");
        private static DocumentClient client = new DocumentClient(new Uri(CosmosWifiEndpointUrl), CosmosPrimaryKey);

        private static string DatabaseName = Environment.GetEnvironmentVariable("CosmosWifiDatabase");
        private static string DataCollectionName = Environment.GetEnvironmentVariable("CosmosHttpCollection");

        [FunctionName("FuncHttp")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //Cosmos DB Database�� Collection�� ������ �����ϴ� �ڵ� ���񽺰� �����Ǹ� ������ �ڵ�
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName });
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseName), new DocumentCollection { Id = DataCollectionName });

            // parse query parameter
            string message = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "message", true) == 0)
                .Value;

            if (message == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                message = data?.name;
            }

            if (message != null || message != "")
            {
                JObject jobjct = JObject.Parse(message);
                Models.HttpModel httpModel = new Models.HttpModel();
                httpModel.id = generateID();
                httpModel.jobject = jobjct;

                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseName, DataCollectionName), httpModel);
            }

            return message == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + message);
        }

        /// ������ �����̸� �����ϱ� ���� �޼ҵ�
        public static string generateID()
        {
            return string.Format("{0}_{1:N}", System.DateTime.Now.Ticks, Guid.NewGuid());
        }

    }
}
