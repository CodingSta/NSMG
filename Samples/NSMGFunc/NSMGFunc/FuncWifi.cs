using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;

using System;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

using System.Data;
using System.Data.SqlClient;

namespace NSMGFunc
{
    public static class FuncWifi
    {
        static private string CosmosWifiEndpointUrl = Environment.GetEnvironmentVariable("CosmosWifiEndpoint");
        static private string CosmosPrimaryKey = Environment.GetEnvironmentVariable("CosmosWifiPrimaryKey");
        private static DocumentClient client =   new DocumentClient(new Uri(CosmosWifiEndpointUrl), CosmosPrimaryKey);

        private static string DatabaseName = Environment.GetEnvironmentVariable("CosmosWifiDatabase");
        private static string DataCollectionName = Environment.GetEnvironmentVariable("CosmosWifiCollection");

        [FunctionName("FuncWifi")]
        public async static void Run([EventHubTrigger("wifi", Connection = "NSMGEventHub")]string myEventHubMessage, TraceWriter log)
        {
            //Cosmos DB Database�� Collection�� ������ �����ϴ� �ڵ� ���񽺰� �����Ǹ� ������ �ڵ�
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName });
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseName), new DocumentCollection { Id = DataCollectionName });
            JArray array = JArray.Parse(myEventHubMessage);

            for(int i=0;i<array.Count;i++)
            {
                string bssid = (string)array[i]["bssid"];
                string ssid = (string)array[i]["ssid"];

                System.Data.SqlClient.SqlParameter[] para = {
                    new System.Data.SqlClient.SqlParameter("bssid", SqlDbType.NVarChar, 17),
                    new System.Data.SqlClient.SqlParameter("ssid", SqlDbType.NVarChar, 50)
                };

                para[0].Value = bssid;
                para[1].Value = ssid;

                //SQL Database�� ������ Matching�ϴ� �κ�
                DataSet ds = Helpers.SQLHelper.RunSQL("SELECT * FROM dbo.bssids WHERE bssid =@bssid AND ssid=@ssid", para);

                //����� ������ Address�� Keyword�� ������Ʈ �Ѵ�. 
                if (ds.Tables[0].Rows.Count != 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    array[i]["keyword"] = row[14].ToString();
                    array[i]["address"] = row[11].ToString();
                }
            }

            //WIFI Models
            Models.WifiModel wifiModel = new Models.WifiModel();
            wifiModel.id = generateID();
            wifiModel.wifies = array;

            await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseName, DataCollectionName), wifiModel);
            
            //��������� ���ؼ� Log ����� �ּ�ó��
            //log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
        }

        /// ������ �����̸� �����ϱ� ���� �޼ҵ�
        public static string generateID()
        {
            return string.Format("{0}_{1:N}", System.DateTime.Now.Ticks, Guid.NewGuid());
        }

    }
}
