namespace PlayFab.AzureFunctions
{
    using Microsoft.Azure.Cosmos.Table;

    public class Player : TableEntity
    {
        public Player() {}
        public Player(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Id = rowKey;
        }

        public string Id { get; private set; }

        public int GameLaunch { get; set; }
    }
}
