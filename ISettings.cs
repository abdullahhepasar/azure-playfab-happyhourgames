namespace PlayFab.AzureFunctions
{
    public interface ISettings
    {
        string FunctionStorageConnectionString { get; }

        string CosmosDbConnectionString { get; }
    }
}
