# Conversational Telemetry - Getting Started

To get started add the following code to your startup.cs file. Text Analytics is used to calculate Sentiment and Key Phrases. This manual call will be removed shortly as LUIS can now do this call for you.

```
// Telemetry Middleware     
string appInsightsKey = Configuration.GetSection("ApplicationInsights.InstrumentationKey").Value;                      

var telemetryMiddleware = new ConversationalTelemetryMiddleware(appInsightsKey, "<TEXTANALYTICSKEYHERE>", "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0");
options.Middleware.Add(dispatcherMiddleware);
```

**That's it!**


These helpers also exist for logging knowledge related telemetry. If you choose to exclude knowledge items from a response (due to low score) you can provide this so we can track firstly a gap in knowledge but the articles that were returned to help with debug. These calls enable you to identify gaps in Knowledge.

```
TelemetryHelper.LogKnowledgeFoundForUnknownQuestion(...);
TelemetryHelper.LogNoKnowledgeForUnknownQuestion(context...);
```
