namespace PlayFab.AzureFunctions
{
    using Microsoft.Azure.Cosmos.Table;
    using System;

    public class Player : TableEntity
    {
        public Player() {}
        public Player(string partitionKey, string guidId)
        {
            PartitionKey = partitionKey;
            RowKey = guidId.ToString();
            Id = guidId;
        }

        public string Id { get; private set; }

        public int GameLaunch { get; set; }
    }
}
