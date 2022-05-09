// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using ProactiveBot.Services;
using ProactiveBot.Models;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class ProactiveBot : ActivityHandler
    {
        // Message to send to users when the bot receives a Conversation Update event
        private const string WelcomeMessage = "ようこそ！　私は、GSS Dev Bot です。";

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        //private readonly ILogger _logger;
        private readonly IConvReferenceTableService _convReferenceTableService;

        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, IConvReferenceTableService convReferenceTableService)
        {
            _conversationReferences = conversationReferences;
            //_logger = logger; , ILogger logger
            _convReferenceTableService = convReferenceTableService;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        private void SaveConversationReference(Activity activity, bool? allowSendMessage = null)
        {
            var conversationReference = activity.GetConversationReference();
            // Conversation Reference Table への書き込み
            var convReferenceItem = new ConvReferenceItem();
            convReferenceItem.RowKey = conversationReference.User.Id;
            convReferenceItem.Name = conversationReference.User.Name;
            convReferenceItem.CoversationReference = JsonConvert.SerializeObject(conversationReference, Formatting.Indented);
            _convReferenceTableService.UpsertEntityAsync(convReferenceItem, allowSendMessage);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            //var message = $"***** こんにちは *****\n<https://www.msn.com/ja-jp/>\nYou sent: {turnContext.Activity.Text}";
            var message = "こんにちは!!<br>- GSS Dev Bot からのメッセージを許容する場合、**OK** と入力してください。<br>- 許容しない場合、**NG** と入力してください。";

            if (turnContext.Activity.Text.ToUpper().Contains("OK"))
            {
                SaveConversationReference(turnContext.Activity as Activity, true);
                message = "メッセージ送信を「可能」に変更しました。";
            }
            else if (turnContext.Activity.Text.ToUpper().Contains("NG"))
            {
                SaveConversationReference(turnContext.Activity as Activity, false);
                message = "メッセージ送信を「不可」に変更しました。";
            }
            else
            {
                SaveConversationReference(turnContext.Activity as Activity);
            }

            var ima = Activity.CreateMessageActivity();
            ima.TextFormat = "markdown"; //"plain" or "markdown"(default)
            ima.Text = message;
            await turnContext.SendActivityAsync(ima);
            // Echo back what the user said
            // await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }
    }
}
