// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

////////// ★Bot Integration with App Insights ////////// begin
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
////////// ★Bot Integration with App Insights ////////// end

using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using ProactiveBot.Services;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, ProactiveBot>();
            ////////// ★App Service Integration with App Insights ////////// begin
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
            ////////// ★App Service Integration with App Insights ////////// end

            ////////// ★Bot Integration with App Insights ////////// begin
            // Create the Bot Framework Adapter with error handling enabled.
            //★services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>(); //★上記にコードが存在しているのでコメント止め

            // Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetry();

            // Create the telemetry client.
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();

            // Add telemetry initializer that will set the correlation context for all telemetry items.
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();

            // Add telemetry initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID)
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();

            // Create the telemetry middleware to initialize telemetry gathering
            services.AddSingleton<TelemetryInitializerMiddleware>();

            // Create the telemetry middleware (used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            ////////// ★Bot Integration with App Insights ////////// end

            services.AddSingleton<ILoggingTableService>(InitializeLoggingTableClientAsync(Configuration).GetAwaiter().GetResult());
            services.AddSingleton<IConvReferenceTableService>(InitializeConvReferenceTableClientAsync(Configuration).GetAwaiter().GetResult());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

        private static async Task<LoggingTableService> InitializeLoggingTableClientAsync(IConfiguration configuration)
        {
            string connectionString = configuration["botStorgeConnectionString"];
            string tableName = configuration["botTableLogging"];
            // Table Client の作成
            var tableClient = new TableClient(connectionString, tableName);
            // Table が存在しなければ作成
            await tableClient.CreateIfNotExistsAsync();
            // Service の作成
            var tableLoggingService = new LoggingTableService(tableClient);

            return tableLoggingService;
        }

        private static async Task<ConvReferenceTableService> InitializeConvReferenceTableClientAsync(IConfiguration configuration)
        {
            string connectionString = configuration["botStorgeConnectionString"];
            string tableName = configuration["botTableConvReference"];
            // Table Client の作成
            var tableClient = new TableClient(connectionString, tableName);
            // Table が存在しなければ作成
            await tableClient.CreateIfNotExistsAsync();
            // Service の作成
            var tableConvReferenceService = new ConvReferenceTableService(tableClient);

            return tableConvReferenceService;
        }
    }
}
