using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab.ServerModels;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace PlayFab.AzureFunctions
{
    public static class HappyHourGamesPlayfab
    {
        private const string DEV_SECRET_KEY = "PLAYFAB_DEV_SECRET_KEY";
        private const string STORAGE_CONNECTION_KEY = "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING";

        [FunctionName("HappyHourGamesPlayfab")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var apiSettings = new PlayFabApiSettings()
            {
                TitleId = context.TitleAuthenticationContext.Id,
                DeveloperSecretKey = System.Environment.GetEnvironmentVariable(DEV_SECRET_KEY)
            };

            PlayFabAuthenticationContext titleContext = new PlayFabAuthenticationContext();
            titleContext.EntityToken = context.TitleAuthenticationContext.EntityToken;
            var serverAPI = new PlayFabServerInstanceAPI(apiSettings, titleContext);

            GetTitleDataRequest titleDataRequest = new GetTitleDataRequest
            {
                Keys = new List<string>(){"key"}
            };

            var titleDataResult = await serverAPI.GetTitleDataAsync(titleDataRequest);
            if(titleDataResult.Result.Data.ContainsKey("key"))
            {
                return titleDataResult.Result.Data;
            }

            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GameLaunchCounter")]
        public static async Task<dynamic> GameLaunchCounter(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            string storageConnectionString = STORAGE_CONNECTION_KEY;
            string tableName = "TableGameLaunchCounter";

            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable cloudTable = tableClient.GetTableReference(tableName);

            string playFabId = context.FunctionArgument.PlayFabId;

            TableOperation retrieveOperation = TableOperation.Retrieve<PlayerEntity>(playFabId, "");
            TableResult result = await cloudTable.ExecuteAsync(retrieveOperation);
            PlayerEntity player = result.Result as PlayerEntity;

            string responseMessage = "1";

                       if (player == null)
            {
                //First Save
                PlayerEntity firstPlayer = new PlayerEntity(playFabId, "")
                {
                    Counter = 1
                };

                MergePlayer(cloudTable, firstPlayer).Wait();
            }
            else
            {
                //Update
                player.Counter += 1;
                responseMessage = player.Counter.ToString();
                MergePlayer(cloudTable, player).Wait();
            }

            return new OkObjectResult(responseMessage);
        }


        public static async Task MergePlayer(CloudTable table, PlayerEntity playerEntity)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(playerEntity);

            //Execute the operation
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            PlayerEntity insertedPlayer = result.Result as PlayerEntity;

            Console.WriteLine("Added/Updated player");
        }
    }

        public class PlayerEntity : TableEntity
    {
        public PlayerEntity() { }

        public PlayerEntity(string entityId, string rowKey)
        {
            PartitionKey = entityId;
            RowKey = rowKey;
        }

        public int Counter { get; set; }
    }
}
