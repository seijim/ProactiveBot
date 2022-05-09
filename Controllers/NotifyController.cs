// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProactiveBot.Services;
using ProactiveBot.Models;
using System.Linq;

namespace ProactiveBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly IConvReferenceTableService _convReferenceTableService;
        private string message;
        private readonly ILogger _logger;

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IConvReferenceTableService convReferenceTableService, ILogger<NotifyController> logger)
        {
            _adapter = adapter;
            _convReferenceTableService = convReferenceTableService;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _logger = logger;
        }

        /// <summary>
        /// プロアクティブメッセージの登録先一覧表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            message = "Proactive message after http-get.";
            string json = string.Empty;

            IEnumerable<ConvReferenceItem> entities;
            try
            {
                entities = await _convReferenceTableService.GetEntitiesAsync();
                json = JsonConvert.SerializeObject(entities, Formatting.Indented);
            }
            catch
            {
            }

            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = $"{json}",
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        /// <summary>
        /// プロアクティブメッセージの送信
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(RequestJsonBody requestJsonBody)
        {
            message = requestJsonBody.Message.ToString();
            string json = string.Empty;
            if (requestJsonBody.ConversationReference != null)
            {
                json = JsonConvert.SerializeObject(requestJsonBody.ConversationReference);
                var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(json);
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            else
            {
                IEnumerable<ConvReferenceItem> entities;
                try
                {
                    entities = await _convReferenceTableService.GetEntitiesAsync();
                    entities = entities.Where(x => x.AllowSendMessage == true);
                    foreach (var entity in entities)
                    {
                        var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(entity.CoversationReference);
                        try
                        {
                            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                        }
                    }
                    _logger.LogInformation(message);
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex.Message);
                }
            }

            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Push(Post) messages have been sent.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(message);
        }
    }

    public class RequestJsonBody
    {
        public string Message { set; get; }
        public JObject ConversationReference { set; get; } 
    }
}
