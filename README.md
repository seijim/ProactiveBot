# プロアクティブ ボット (Proactive Bot) sample

## 概要

[Proactive Bot sample](https://github.com/microsoft/botbuilder-samples.git) をベースに、Conversation Reference を Azure Table Storage に保存できるようにして、チャットの問いで「送信を許可」したユーザー全員に対して、ボットからのプロアクティブ メッセージの送信を可能にするサンプルコードです。

## 前提条件

- [.NET SDK](https://dotnet.microsoft.com/download) version 6

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## appsettings.json の編集

Application Insights の接続文字列、Azure Storage の接続文字列を設定してください。

  ```bash
  {
    "MicrosoftAppType": "",
    "MicrosoftAppId": "",
    "MicrosoftAppPassword": "",
    "MicrosoftAppTenantId": "",
    "ApplicationInsights": {
      "ConnectionString": "<your_application_insights_connection_string>",
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning"
      }
    },
    "Logging": {
      "LogLevel": {
        // Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5, None = 6
        "Default": "Warning", // Default logging, Error and higher.
        "Microsoft": "Error" // All Microsoft* categories, Warning and higher.
      }
    },
    "botStorgeConnectionString": "<your_storage_account_connection_string>",
    "botTableLoggingEnable": "true",
    "botTableLogging": "logging",
    "botTableConvReference": "convReference"
  }
  ```

## 参照

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Send proactive messages](https://docs.microsoft.com/ja-jp/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=js)
- [continueConversation Method](https://docs.microsoft.com/ja-jp/javascript/api/botbuilder/botframeworkadapter#continueconversation)
- [getConversationReference Method](https://docs.microsoft.com/ja-jp/javascript/api/botbuilder-core/turncontext#getconversationreference)
- [Activity processing](https://docs.microsoft.com/ja-jp/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/ja-jp/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/ja-jp/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/ja-jp/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
