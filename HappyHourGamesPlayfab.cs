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

using static PlayFab.AzureFunctions.Constants;

namespace PlayFab.AzureFunctions
{
    public static class HappyHourGamesPlayfab
    {
        [FunctionName("GameLaunchCounter")]
        public static async Task<dynamic> GameLaunchCounter(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
        
            string playFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            string tableName = ATNGameLaunchCounter;
            Player player = await AzureTableRepository.ReadAsync<Player>(tableName, TablePartitionKey, playFabId);
            
            if(player == null)
            {
                //First Save
                 Player newPlayer = new Player(TablePartitionKey, playFabId)
                {
                    GameLaunch = 1
                };

                player = newPlayer;
            }
            else
            {
                //Update
                player.GameLaunch += 1;
            }

            await AzureTableRepository.InsertOrMerge(tableName, player);
            
            string responseMessage = player.GameLaunch.ToString();

            return new OkObjectResult(responseMessage);
        }
    }
}
