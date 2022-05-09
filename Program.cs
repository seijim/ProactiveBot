// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Microsoft.BotBuilderSamples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((logging) =>
                    {
                        logging.AddDebug();
                        logging.AddConsole();
                    });
                    webBuilder.UseStartup<Startup>();
                });
                 /****** コードでログレベルを指定する場合
                .ConfigureLogging((context, builder) =>
                {
                    ////////// ★App Service Integration with App Insights ////////// begin
                    // Capture all log-level entries from Program
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

                    // Capture all log-level entries from Startup
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Error);
                    ////////// ★App Service Integration with App Insights ////////// end
                });
                ******/
    }
}
