using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Newtonsoft.Json;
using ProactiveBot.Models;
using System.Text;
using System.Linq;

namespace ProactiveBot.Services
{
    public interface IConvReferenceTableService
    {
        Task<ConvReferenceItem> UpsertEntityAsync(ConvReferenceItem entity, bool? allowSendMessage = null);
        Task DeleteEntityAsync(string rowKey);
        Task<ConvReferenceItem> GetEntityAsync(string rowKey);
        Task<IEnumerable<ConvReferenceItem>> GetEntitiesAsync();
    }

    public class ConvReferenceTableService : IConvReferenceTableService
    {
        private readonly TableClient _tableClient;

        public ConvReferenceTableService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        /// <summary>
        /// 会話リファレンスを Upsert (作成/更新) する
        /// </summary>
        /// <returns></returns>
        public async Task<ConvReferenceItem> UpsertEntityAsync(ConvReferenceItem entity, bool? allowSendMessage = null)
        {
            ConvReferenceItem existedEntity;
            entity.PartitionKey = entity.RowKey.Substring(0, 4);
            if (allowSendMessage is null)
            {
                try
                {
                    existedEntity = await GetEntityAsync(entity.RowKey);
                    entity.AllowSendMessage = existedEntity.AllowSendMessage;
                }
                catch(Exception ex)
                {
                    string msg = ex.Message;
                    entity.AllowSendMessage = false;
                }
            }
            else
            {
                entity.AllowSendMessage = (bool)allowSendMessage;
            }

            await _tableClient.UpsertEntityAsync(entity);
            return entity;
        }

        /// <summary>
        /// 特定ユーザーの会話リファレンスを削除する
        /// </summary>
        /// <returns></returns>
        public async Task DeleteEntityAsync(string rowKey)
        {
            var partitionKey = rowKey.Substring(0, 4);
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        /// <summary>
        /// 特定ユーザーの会話リファレンスを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<ConvReferenceItem> GetEntityAsync(string rowKey)
        {
            var partitionKey = rowKey.Substring(0, 4);
            return await _tableClient.GetEntityAsync<ConvReferenceItem>(partitionKey, rowKey);
        }

        /// <summary>
        /// すべての会話リファレンスを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ConvReferenceItem>> GetEntitiesAsync()
        {
            var entitiesMaxPerPage = _tableClient.QueryAsync<ConvReferenceItem>(filter: "");
            await foreach (var page in entitiesMaxPerPage.AsPages())
            {
                return page.Values;
            }

            return null;
        }
    }
}
