using System;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Newtonsoft.Json;

namespace ProactiveBot.Models
{
    public class LoggingItem : ITableEntity
    {
        [JsonProperty(PropertyName = "requestHeaders")]
        public string RequestHeaders { get; set; }
        [JsonProperty(PropertyName = "requestBody")]
        public string RequestBody { get; set; }
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
