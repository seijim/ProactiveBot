// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
// Request のダンプ
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using ProactiveBot.Services;
using ProactiveBot.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly IConfiguration _configuration;
        private readonly ILoggingTableService _loggingTableService;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot, IConfiguration configuration, ILoggingTableService loggingTableService)
        {
            _adapter = adapter;
            _bot = bot;
            _configuration = configuration;
            _loggingTableService = loggingTableService;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Request message の Table Storage への格納
            try
            {
                // リクエスト Body を繰り返し読めるように設定
                Request.EnableBuffering();
                var jsonHeaders = JsonConvert.SerializeObject(Request.Headers);
                string jsonBody;
                // ストリームを破棄しないように設定
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, -1, true))
                {
                    // リクエスト Body を読み込む
                    jsonBody = await reader.ReadToEndAsync();

                    // 読み込み開始位置を先頭に戻しておく
                    reader.BaseStream.Position = 0;
                    //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Logging Table への書き込み
                if (_configuration["botTableLoggingEnable"] == "true")
                    await _loggingTableService.LogData(jsonHeaders, jsonBody);
                var result = await _loggingTableService.GetEntitiesAsync();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }

            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }

}
