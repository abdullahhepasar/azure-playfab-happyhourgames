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
using Microsoft.Extensions.DependencyInjection;

using static PlayFab.AzureFunctions.Constants;

namespace PlayFab.AzureFunctions
{
    public static class HappyHourGamesPlayfab
    {
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
        
            var settings = new PlayFabApiSettings
            {
                TitleId = context.TitleAuthenticationContext.Id,
                DeveloperSecretKey = Environment.GetEnvironmentVariable(DEV_SECRET_KEY, EnvironmentVariableTarget.Process),
            };
        
            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = context.TitleAuthenticationContext.EntityToken
            };
        
            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);
        
            var result = await  serverApi.GetUserInventoryAsync(new GetUserInventoryRequest()
            {
                PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
            });

            string playFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            var container = IoCContainer.Create();
            //var azureTableRepository = container.GetRequiredService<IAzureTableRepository>();
            var azureTableRepository = container.GetRequiredService<IAzureTableRepository>();

            Player player = new Player(TablePartitionKey, playFabId)
            {
                GameLaunch = 1
            };

            var playerUpdate = await azureTableRepository.UpsertAsync(ATNGameLaunchCounter, player);

            string responseMessage = "SUCCESS: UPDATED -> " + playFabId + "->Other Player Data:" + result;

            return new OkObjectResult(responseMessage);
        }
    }
}
