namespace PlayFab.AzureFunctions
{
    using Microsoft.Azure.Cosmos.Table;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;

    using static PlayFab.AzureFunctions.Constants;

    public static class AzureTableRepository
    {

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns></returns>
        public static async Task<T> ReadAsync<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity
        {
            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            TableResult result = await table.ExecuteAsync(retrieveOperation).ConfigureAwait(false);

            return result.Result as T;
        }

        public static async Task<IEnumerable<T>> ReadAllAsync<T>(string tableName) where T : TableEntity, new()
        {
            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableContinuationToken token = null;
            var entities = new List<T>();
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<T>(), token).ConfigureAwait(false);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        /// <summary>
        /// Adds the or update entity.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static async Task<object> UpsertAsync(string tableName, TableEntity entity)
        {
            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

            TableResult result = await table.ExecuteAsync(insertOrReplaceOperation).ConfigureAwait(false);

            return result.Result;
        }

        /// <summary>
        /// Update or Add the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns></returns>
        public static async Task<object> InsertOrMerge(string tableName, TableEntity entity)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);

            return result.Result;
        }

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static async Task<object> DeleteAsync(string tableName, TableEntity entity)
        {
            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableOperation deleteOperation = TableOperation.Delete(entity);

            TableResult result = await table.ExecuteAsync(deleteOperation).ConfigureAwait(false);

            return result.Result;
        }

        /// <summary>
        /// Add the entity.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static async Task<object> AddAsync(string tableName, TableEntity entity)
        {
            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableOperation insertOperation = TableOperation.Insert(entity);

            TableResult result = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);

            return result.Result;
        }

      
        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static async Task<object> UpdateAsync(string tableName, TableEntity entity)
        {
            var table = await CreateTableIfNotExists(tableName).ConfigureAwait(false);

            TableOperation replaceOperation = TableOperation.Replace(entity);

            TableResult result = await table.ExecuteAsync(replaceOperation).ConfigureAwait(false);

            return result.Result;
        }

        /// <summary>
        /// Ensures existence of the table.
        /// </summary>
        private static async Task<CloudTable> CreateTableIfNotExists(string tableName)
        {
            CloudStorageAccount storageAccount;
            var storageConnectionString = Environment.GetEnvironmentVariable(STORAGE_CONNECTION_KEY, EnvironmentVariableTarget.Process);
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            return table;
        }

        /// <summary>
        /// Gets entities by query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<T>> QueryAsync<T>(CloudTable table, TableQuery<T> query)
            where T : class, ITableEntity, new()
        {
            var entities = new List<T>();

            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }
    }
}
