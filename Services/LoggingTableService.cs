using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Newtonsoft.Json;
using ProactiveBot.Models;

namespace ProactiveBot.Services
{
    public interface ILoggingTableService
    {
        Task<LoggingItem> UpsertEntityAsync(LoggingItem entity);
        Task DeleteEntityAsync(string partitionKey, string rowKey);
        Task<LoggingItem> GetEntityAsync(string partitionKey, string rowKey);
        Task<IEnumerable<LoggingItem>> GetEntitiesAsync();
        Task<IEnumerable<LoggingItem>> GetEntitiesByPartitionAsync(string partitionKey);
        Task LogData(string requestHeaders, string requestBody);
    }

    public class LoggingTableService : ILoggingTableService
    {
        private readonly TableClient _tableClient;

        public LoggingTableService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        /// <summary>
        /// エンティティを Upsert (作成/更新) する
        /// </summary>
        /// <returns></returns>
        public async Task<LoggingItem> UpsertEntityAsync(LoggingItem entity)
        {
            await _tableClient.UpsertEntityAsync(entity);
            return entity;
        }

        /// <summary>
        /// 特定のエンティティを削除する
        /// </summary>
        /// <returns></returns>
        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        /// <summary>
        /// 特定のエンティティを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<LoggingItem> GetEntityAsync(string partitionKey, string rowKey)
        {
            return await _tableClient.GetEntityAsync<LoggingItem>(partitionKey, rowKey);
        }

        /// <summary>
        /// すべてのエンティティを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<LoggingItem>> GetEntitiesAsync()
        {
            var entitiesMaxPerPage = _tableClient.QueryAsync<LoggingItem>(filter: "", maxPerPage: 10);
            await foreach (var page in entitiesMaxPerPage.AsPages())
            {
                return page.Values;
            }

            return null;
        }

        /// <summary>
        /// 特定のパーティションのエンティティを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<LoggingItem>> GetEntitiesByPartitionAsync(string partitionKey)
        {
            //var eitiesMaxPerPage = _tableClient.QueryAsync<LoggingItem>(filter: $"PartitionKey eq '{name}'", maxPerPage: 10);
            var entitiesMaxPerPage = _tableClient.QueryAsync<LoggingItem>(x => x.PartitionKey == partitionKey, maxPerPage: 10);
            await foreach (var page in entitiesMaxPerPage.AsPages())
            {
                return page.Values;
            }

            return null;
        }

        /// <summary>
        /// エンティティを構成し、Upsert (作成/更新) する
        /// </summary>
        /// <returns></returns>
        public async Task LogData(string requestHeaders, string requestBody)
        {
            var entity = new LoggingItem();
            var today = DateTimeOffset.UtcNow;
            entity.PartitionKey = String.Format("{0:d04}{1:d02}{2:d02}", today.Year, today.Month, today.Day);
            entity.RowKey = String.Format("{0:d04}{1:d02}{2:d02}{3:d02}{4:d02}{5:d02}{6:d04}", today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Millisecond) + "_" + Guid.NewGuid().ToString();
            entity.RequestHeaders = requestHeaders;
            entity.RequestBody = requestBody;
            await _tableClient.UpsertEntityAsync(entity);
        }
    }
}
