using System;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Newtonsoft.Json;
using Microsoft.Bot.Schema;

namespace ProactiveBot.Models
{
    public class ConvReferenceItem : ITableEntity
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "allowSendMessage")]
        public bool AllowSendMessage { get; set; }
        [JsonProperty(PropertyName = "conversationReference")]
        public string CoversationReference { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        [JsonProperty(PropertyName = "rowKey")]
        public string RowKey { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public DateTimeOffset? Timestamp { get; set; }
        [JsonProperty(PropertyName = "etag")]
        public Azure.ETag ETag { get; set; }
    }
}
