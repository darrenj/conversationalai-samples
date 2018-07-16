//-----------------------------------------------------------------------
// <copyright file="CommonCoreTelemetryMiddleware.cs" company="Microsoft">
//     Copyright (c) 2017 Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using ConversationalTelemetry;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using System;
using System.Threading.Tasks;

namespace CommonCoreTelemetry
{
    public class ConversationalTelemetryMiddleware : IMiddleware
    {
        public ConversationalTelemetryMiddleware(string AppInsightsKey, string TextAnalyticsKey, string TextAnalyticsEndpoint, bool LogOriginalMessages = true, bool LogUserName = true, bool LogSentimentAndKeyPhrases = true, int SentimentWordThreshold = 3)
        {
            // Move to passing config entries in to better support v4 Middleware and .NET Core config changes
            appInsightsKey = AppInsightsKey;
            textAnalyticsKey = TextAnalyticsKey;
            logOriginalMessages = LogOriginalMessages;
            logUserName = LogUserName;
            logSentimentAndKeyPhrases = LogSentimentAndKeyPhrases;
            sentimentWordThreshold = SentimentWordThreshold;
            textAnalyticsEndpoint = TextAnalyticsEndpoint;
        }

        #region Config settings
        private string appInsightsKey;
        private string textAnalyticsKey;
        private string textAnalyticsEndpoint;
        private bool logOriginalMessages;
        private bool logUserName;
        private bool logSentimentAndKeyPhrases;
        private int sentimentWordThreshold;
        #endregion

        // We log each message in a consistent way within Application Insights along with LUIS intent information in order
        // to provide a consistent set of insights across all Bots using a baseline PowerBI dashboard.
        // This middleware comoponent always passes on to the next in the pipeline
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            TelemetryHelper telemetryHelper = new TelemetryHelper(appInsightsKey, textAnalyticsKey, textAnalyticsEndpoint, logOriginalMessages, logUserName, logSentimentAndKeyPhrases, sentimentWordThreshold);

            try
            {                
                // We create an Event for each message coming in (regardless of LUIS invocation)
                telemetryHelper.LogIncomingMessage(context);

                // Protect against LUIS not having been called
                var luisResult = context.Services.Get<RecognizerResult>
                   (LuisRecognizerMiddleware.LuisRecognizerResultKey);
               
                if (luisResult != null)
                {                
                    // We also log a seperate intent to store the LUIS results
                    telemetryHelper.LogIntent(context);
                }
            }
            catch (Exception e)
            {
                telemetryHelper.TrackException(e);
            }

            // On to the next component
            await next();
        }
    }
}
