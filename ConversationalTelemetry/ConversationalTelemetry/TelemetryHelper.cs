//-----------------------------------------------------------------------
// <copyright file="TelemetryHelper.cs" company="Microsoft">
//     Copyright (c) 2017 Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace ConversationalTelemetry
{
    using ConversationalTelemetry.Helpers;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Ai.LUIS;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Rest;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Simple helper for creating telemetry in a consistent way for a dashboard
    /// </summary>
    public class TelemetryHelper
    {    
        public TelemetryHelper(string AppInsightsKey, string TextAnalyticsKey, string TextAnalyticsEndpoint, bool LogOriginalMessages = true, bool LogUserName = true, bool LogSentimentAndKeyPhrases = true, int SentimentWordThreshold = 3)
        {           
            appInsightsKey = AppInsightsKey;
            textAnalyticsKey = TextAnalyticsKey;
            textAnalyticsEndpoint = TextAnalyticsEndpoint;
            logOriginalMessages = LogOriginalMessages;
            logUserName = LogUserName;
            logSentimentAndKeyPhrases = LogSentimentAndKeyPhrases;
            sentimentWordThreshold = SentimentWordThreshold;

            if (string.IsNullOrEmpty(appInsightsKey))
            {
                throw new ArgumentNullException("A value for 'AppInsightsKey' was not passed.");
            }          

            if (string.IsNullOrEmpty(textAnalyticsKey))
            {
                throw new ArgumentNullException("A value for 'TextAnalyticsKey' was not passed");
            }

            if (string.IsNullOrEmpty(textAnalyticsEndpoint))
            {
                throw new ArgumentNullException("A value for 'textAnalyticsEndpoint' could not be found in application settings");
            }

            textAnalyticsClient = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(textAnalyticsKey));
            textAnalyticsClient.BaseUri = new Uri(textAnalyticsEndpoint);
        }    

        #region Config settings
        private string appInsightsKey;
        private string textAnalyticsKey;
        private string textAnalyticsEndpoint;
        private bool logOriginalMessages;
        private bool logUserName;
        private bool logSentimentAndKeyPhrases;
        private int sentimentWordThreshold;
        private TextAnalyticsClient textAnalyticsClient;
        #endregion

        #region Other const values

        /// <summary>
        /// Prefix string for logging Luis intents
        /// </summary>
        private const string IntentPrefix = "LuisIntent";

        /// <summary>
        /// The prefix string for a custom event, so that it can be distinguished with events of the same name that may be used in this helper class.
        /// </summary>
        private const string CustomEventPrefix = "CustomEvent";
        
        /// <summary>
        /// The default sentiment word threshold. This value indicates the minimum number of words in a query before attempting to analyze sentiment.
        /// </summary>
        private const int DefaultSentimentWordThreshold = 3;

        /// <summary>
        /// The client info identifier.
        /// </summary>
        private const string ClientInfoIdentifier = "clientInfo";

        #endregion

        #region Telemetry Events

        /// <summary>
        /// The bot message received event.
        /// </summary>
        private const string BotMessageReceivedEvent = "BotMessageReceived";
        
        /// <summary>
        /// The button press event.
        /// </summary>
        private const string ButtonPressEvent = "ButtonPress";

        /// <summary>
        /// The KnowledgeBaseQuestion event.
        /// </summary>
        private const string KBQuestionEvent = "KBQuestion";

        /// <summary>
        /// The QNAResponseFound event.
        /// </summary>
        private const string QnAResponseFoundEvent = "QnAResponseFound";

        /// <summary>
        /// The Error event.
        /// </summary>
        private const string ErrorEvent = "Error";

        #endregion

        #region Telemetry Properties

        /// <summary>
        /// The Channel property.
        /// </summary>
        private const string ChannelProperty = "Channel";

        /// <summary>
        /// The FromId property.
        /// </summary>
        private const string FromIdProperty = "FromId";

        /// <summary>
        /// The FromName property.
        /// </summary>
        private const string FromNameProperty = "FromName";

        /// <summary>
        /// The ConversationId property.
        /// </summary>
        private const string ConversationIdProperty = "ConversationId";

        /// <summary>
        /// The ConversationName property.
        /// </summary>
        private const string ConversationNameProperty = "ConversationName";

        /// <summary>
        /// The ClientInfo property.
        /// </summary>
        private const string ClientInfoProperty = "ClientInfo";

        /// <summary>
        /// The Text property.
        /// </summary>
        private const string TextProperty = "Text";

        /// <summary>
        /// The Locale property.
        /// </summary>
        private const string LocaleProperty = "Locale";

        /// <summary>
        /// The Language property.
        /// </summary>
        private const string LanguageProperty = "Language";

        /// <summary>
        /// The Sentiment property.
        /// </summary>
        private const string SentimentProperty = "Sentiment";

        /// <summary>
        /// The Key Phrases property.
        /// </summary>
        private const string KeyPhrasesProperty = "KeyPhrases";

        /// <summary>
        /// The IntentScore property.
        /// </summary>
        private const string IntentScoreProperty = "Score";

        /// <summary>
        /// The ConfidenceScore property.
        /// </summary>
        private const string ConfidenceScoreProperty = "ConfidenceScore";

        /// <summary>
        /// The Question property.
        /// </summary>
        private const string QuestionProperty = "Question";

        /// <summary>
        /// The FoundInKnowledgeSource property.
        /// </summary>
        private const string FoundInKnowledgeSourceProperty = "FoundInKnowledgeSource";

        /// <summary>
        /// The KnowledgeBasedUsed property.
        /// </summary>
        private const string KnowledgeBasedUsedProperty = "KnowledgeBasedUsed";

        /// <summary>
        /// The UserAcceptedAnswer property.
        /// </summary>
        private const string UserAcceptedAnswerProperty = "UserAcceptedAnswer";

        /// <summary>
        /// The Intent property.
        /// </summary>
        private const string IntentProperty = "Intent";

        /// <summary>
        /// The ButtonValue property.
        /// </summary>
        private const string ButtonValueProperty = "ButtonValue";

        /// <summary>
        /// The KnowledgeItemsDiscarded property.
        /// </summary>
        private const string KnowledgeItemsDiscardedProperty = "KnowledgeItemsDiscarded";

        /// <summary>
        /// The QNAResponse property.
        /// </summary>
        private const string QnAResponseProperty = "QnAResponse";

        /// <summary>
        /// The Error property.
        /// </summary>
        private const string ErrorProperty = "Error";

        /// <summary>
        /// The ErrorHeadline property.
        /// </summary>
        private const string ErrorHeadlineProperty = "ErrorHeadline";

        /// <summary>
        /// The ErrorData property.
        /// </summary>
        private const string ErrorDataProperty = "ErrorData";

        /// <summary>
        /// The NoResponseGiven property.
        /// </summary>
        private const string NoResponseGivenProperty = "NoResponseGiven";

        #endregion

        /// <summary>
        /// Log a custom event with the given name, properties and metrics.
        /// This acts as a simple wrapper around the TelemetryClient.TrackEvent method.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">The telemetry properties.</param>
        /// <param name="metrics">The metrics.</param>
        public void TrackEvent(string eventName, Dictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            // Simple guard statements
            EnsureNotNullOrEmpty(eventName, "Property eventName must be set");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                // Use the prefix string for a custom event, so that it can be distinguished with events of the same name that may be used in this helper class.
                tc.TrackEvent($"{TelemetryHelper.CustomEventPrefix}.{eventName}", properties, metrics);
            });
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// This acts as a simple wrapper around the TelemetryClient.TrackTrace method.
        /// </summary>
        /// <param name="traceString">The trace string.</param>
        /// <param name="severityLevel">The severity level.</param>
        /// <param name="properties">The properties.</param>
        public void Trace(string traceString, SeverityLevel severityLevel = SeverityLevel.Information, Dictionary<string, string> properties = null)
        {
            // Simple guard statements
            EnsureNotNullOrEmpty(traceString, "Property traceString must be set");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                tc.TrackTrace(traceString, severityLevel, properties);
            });
        }

        /// <summary>
        /// Send a track exception message for display in Diagnostic Search.
        /// This acts as a simple wrapper around the TelemetryClient.TrackException method.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="metrics">The metrics.</param>
        public void TrackException(Exception ex, Dictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            // Simple guard statements
            EnsureNotNull(ex, "Property exception must be set");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                tc.TrackException(ex, properties, metrics);
            });
        }

        /// <summary>
        /// Logs an incoming message to the Bot. Core channel information is stored along with the original message and sentiment.
        /// Logs an event called "BotMessageReceived".
        /// Logs the following properties: conversation id, from user id, conversation name, channel id, client info.
        /// Other info logged based on configuration: from user name, original message, sentiment.
        /// </summary>
        /// <param name="activity">The incoming Activity from the Bot Framework.</param>
        /// <param name="additionalProperties">Additional properties to log.</param>
        public void LogIncomingMessage(ITurnContext context, Dictionary<string, string> additionalProperties = null)
        {
            // Simple guard statements
            EnsureNotNull(context, "Context activity cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                var activity = context.Activity.AsMessageActivity();
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();

                // Set the Telemetry context as we don't have a BF Telemetry Initializer at this time...
                if (!string.IsNullOrEmpty(activity.Conversation.Id))
                {
                    tc.Context.Session.Id = activity.Conversation.Id;
                }

                if (!string.IsNullOrEmpty(activity.From.Id))
                {
                    tc.Context.User.Id = activity.From.Id;
                }

                // General message metadata, defensive to cover for discrepancies across channels
                if (!string.IsNullOrEmpty(activity.ChannelId))
                {
                    telemetryProperties.Add(TelemetryHelper.ChannelProperty, activity.ChannelId);
                }

                if (!string.IsNullOrEmpty(activity.From.Id))
                {
                    telemetryProperties.Add(TelemetryHelper.FromIdProperty, activity.From.Id);
                }

                if (!string.IsNullOrEmpty(activity.Conversation.Id))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, activity.Conversation.Id);
                }

                if (!string.IsNullOrEmpty(activity.Conversation.Name))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationNameProperty, activity.Conversation.Name);
                }

                if (additionalProperties != null)
                {
                    foreach (KeyValuePair<string, string> valuePair in additionalProperties)
                    {
                        if (telemetryProperties.ContainsKey(valuePair.Key))
                        {
                            telemetryProperties[valuePair.Key] = valuePair.Value;
                        }
                        else
                        {
                            telemetryProperties.Add(valuePair.Key, valuePair.Value);
                        }
                    }
                }

                // Now check for specific entities such as client info...
                if (activity.Entities != null)
                {

                    // Do we have any client info? e.g. language, locale, device type
                    var clientEntity = activity.Entities.FirstOrDefault(e => e.Type == TelemetryHelper.ClientInfoIdentifier);

                    if (clientEntity != null && clientEntity.Properties != null)
                    {                      
                        string prop = clientEntity.Properties.ToString();
                        telemetryProperties.Add(TelemetryHelper.ClientInfoProperty, prop);                      
                    }
                }

                // For some customers, logging user name within Application Insights might be an issue so have provided a config setting to disable this feature
                if (logUserName && !string.IsNullOrEmpty(activity.From.Name))
                {
                    telemetryProperties.Add(TelemetryHelper.FromNameProperty, activity.From.Name);
                }

                // For some customers, logging the utterances within Application Insights might be an so have provided a config setting to disable this feature
                if (logOriginalMessages && !string.IsNullOrEmpty(activity.Text))
                {
                    telemetryProperties.Add(TelemetryHelper.TextProperty, activity.Text);
                }

                // Only perform Text Analytics operations if we have a valid key and log sentiment is set to true
                if (!string.IsNullOrEmpty(textAnalyticsKey) && logSentimentAndKeyPhrases && !string.IsNullOrEmpty(activity.Text))
                {
                    try
                    {
                        // Crude but gets a good sense of the utterance length.
                        // NOTE: before splitting, replace multiple instances of spaces with a single space 
                        // and trim either end, so that we do not skew the amount of words in the trimmed list.
                        string modifiedText = Regex.Replace(activity.Text, @"\s+", " ").Trim();
                        string[] words = activity.Text.Split(' ');

                        // Sentiment and Key Phrase extraction is not effective on short utterances so we skip if less than the provided threshold
                        if (words.Length >= sentimentWordThreshold)
                        {                          
                            // For each utterance we identify the language in order to then extract key-phrases and sentiment which is addded to the Telemetry
                            // LUIS now performs Key Phrase Extraction and Sentiment for you but not exposed as part of the v4SDK, once fixed this can be removed.
                            var (identifiedLanguage, keyPhrases, sentiment) = TextAnalyticsHelper.EvaluateUtterance(textAnalyticsClient, activity.Text);

                            if (!string.IsNullOrEmpty(identifiedLanguage))
                            {
                                telemetryProperties.Add(TelemetryHelper.LanguageProperty, identifiedLanguage);
                            }

                            if (!string.IsNullOrEmpty(keyPhrases))
                            {
                                telemetryProperties.Add(TelemetryHelper.KeyPhrasesProperty, keyPhrases);
                            }

                            if (sentiment != int.MinValue)
                            {
                                string sentimentScore = sentiment.ToString("N2");
                                telemetryProperties.Add(TelemetryHelper.SentimentProperty, sentimentScore);
                            }
                        }
                        else
                        {
                            tc.TrackTrace($"TelemetryHelper::LogIncomingMessage::No sentiment calculated for a utterance with {words.Length} word(s).");
                        }
                    }
                    catch (Exception e)
                    {
                        tc.TrackException(e);
                        tc.TrackTrace($"TelemetryHelper::Exception ocurred whilst calculating sentiment - skipping but still logging without it. {e.Message}");
                    }
                }               

                // Log the event
                tc.TrackEvent(TelemetryHelper.BotMessageReceivedEvent, telemetryProperties);
            });
        }

        /// <summary>
        /// Logs an event that a Luis Intent has been identified.
        /// Logs an event called "LuisIntent.{TopScoringIntent.Intent}".
        /// </summary>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="result">The Luis result.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        public void LogIntent(ITurnContext context, Dictionary<string, string> additionalProperties = null)
        {
            // Simple guard statements
            EnsureNotNull(context, "Context result cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                string conversationId = context.Activity.Conversation.Id;

                var luisResult = context.Services.Get<RecognizerResult>
                   (LuisRecognizerMiddleware.LuisRecognizerResultKey);

                if (luisResult != null)
                {
                    var topLuisIntent = luisResult.GetTopScoringIntent();
                    string intentScore = topLuisIntent.score.ToString("N2");

                    // Add the intent score and conversation id properties
                    Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                    telemetryProperties.Add(TelemetryHelper.IntentProperty, topLuisIntent.intent);
                    telemetryProperties.Add(TelemetryHelper.IntentScoreProperty, intentScore);

                    if (!string.IsNullOrEmpty(conversationId))
                    {
                        telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, conversationId);
                    }

                    #region Evaluate if there is a need to add Entitites
                    /*
                    var entities = new List<string>();
                    foreach (var entity in recognizerResult.Entities)
                    {
                        if (!entity.Key.ToString().Equals("$instance"))
                        {
                            entities.Add($"{entity.Key}: {entity.Value.First}");
                        }
                    }
                    */
                    #endregion

                    if (additionalProperties != null)
                    {
                        foreach (KeyValuePair<string, string> valuePair in additionalProperties)
                        {
                            if (telemetryProperties.ContainsKey(valuePair.Key))
                            {
                                telemetryProperties[valuePair.Key] = valuePair.Value;
                            }
                            else
                            {
                                telemetryProperties.Add(valuePair.Key, valuePair.Value);
                            }
                        }
                    }

                    // For some customers, logging user name within Application Insights might be an issue so have provided a config setting to disable this feature     
                    if (logOriginalMessages && !string.IsNullOrEmpty(context.Activity.Text))
                    {
                        telemetryProperties.Add(TelemetryHelper.QuestionProperty, context.Activity.Text);
                    }

                    // Track the event
                    tc.TrackEvent($"{TelemetryHelper.IntentPrefix}.{topLuisIntent.intent}", telemetryProperties);
                }
            });
        }

        /// <summary>
        /// Logs a button press (i.e. a CardAction) within the Bot.
        /// Logs an event called "ButtonPress".
        /// </summary>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="buttonValue">The button value (e.g. either the actual button text or the payload that was executed on the press).</param>
        /// <param name="intent">An optional value to log the intent/ resulting action of the button.</param>
        /// <param name="additionalProperties">Any additional properties to log with the event.</param>
        public void LogButtonPress(string conversationId, string buttonValue, string intent = null, Dictionary<string, string> additionalProperties = null)
        {
            // Simple guard statements
            EnsureNotNullOrEmpty(buttonValue, "Property buttonValue cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(buttonValue))
                {
                    telemetryProperties.Add(TelemetryHelper.ButtonValueProperty, buttonValue);
                }

                if (!string.IsNullOrEmpty(intent))
                {
                    telemetryProperties.Add(TelemetryHelper.IntentProperty, intent);
                }

                if (!string.IsNullOrEmpty(conversationId))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, conversationId);
                }

                if (additionalProperties != null)
                {
                    foreach (KeyValuePair<string, string> valuePair in additionalProperties)
                    {
                        if (telemetryProperties.ContainsKey(valuePair.Key))
                        {
                            telemetryProperties[valuePair.Key] = valuePair.Value;
                        }
                        else
                        {
                            telemetryProperties.Add(valuePair.Key, valuePair.Value);
                        }
                    }
                }

                // Store an event called ButtonPress and attach the properties enabling simple filtering.
                tc.TrackEvent(TelemetryHelper.ButtonPressEvent, telemetryProperties);
            });
        }

        /// <summary>
        /// Logs an event indicating that some knowledge (e.g. QNA response or document) was found for the given question.
        /// Logs an event called "KnowledgeBQuestion".
        /// </summary>
        /// <param name="context">The conversation context</param>
        /// <param name="knowledgeBasedUsed">An identifier for the knowledge base used.</param>
        /// <param name="userAccepted">A flag indicating whether or not the user approved/ accepted the answer as a correct response.</param>
        public void LogKnowledgeFoundForUnknownQuestion(ITurnContext context, string knowledgeBasedUsed = null, bool? userAccepted = null)
        {
            // Simple guard statements
            EnsureNotNull(context, "Context cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                telemetryProperties.Add(TelemetryHelper.QuestionProperty, context.Activity.Text);
                telemetryProperties.Add(TelemetryHelper.FoundInKnowledgeSourceProperty, "True");

                if (!string.IsNullOrEmpty(context.Activity.Conversation.Id))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, context.Activity.Conversation.Id);
                }

                if (!string.IsNullOrEmpty(knowledgeBasedUsed))
                {
                    telemetryProperties.Add(TelemetryHelper.KnowledgeBasedUsedProperty, knowledgeBasedUsed);
                }

                if (userAccepted.HasValue)
                {
                    // In some cases we ask the user if that helped, if we did we store the result here to measure effectiveness         
                    telemetryProperties.Add(TelemetryHelper.UserAcceptedAnswerProperty, userAccepted.Value.ToString());
                }

                // Store an event called KBQuestion and attach the properties enabling simple filtering.     
                tc.TrackEvent(TelemetryHelper.KBQuestionEvent, telemetryProperties);
            });
        }

        /// <summary>
        /// Logs an event indicating that some knowledge (specifically from QNA) was found for the given question.
        /// Logs an event called "QNAResponseFound".
        /// </summary>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="question">The question.</param>
        /// <param name="qnaResponse">The QNA response that was returned.</param>
        /// <param name="confidenceScore">The confidence score of the result.</param>
        public void LogQnAResponseFound(string conversationId, string question, string qnaResponse, double confidenceScore)
        {
            // Simple guard statements
            EnsureNotNullOrEmpty(question, "Property question cannot be null");
            EnsureNotNullOrEmpty(qnaResponse, "Property qnaResponse cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                telemetryProperties.Add(TelemetryHelper.QuestionProperty, question);
                telemetryProperties.Add(TelemetryHelper.QnAResponseProperty, qnaResponse);
                telemetryProperties.Add(TelemetryHelper.ConfidenceScoreProperty, confidenceScore.ToString("N2"));

                if (!string.IsNullOrEmpty(conversationId))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, conversationId);
                }

                // Store an event called QnAResponseFound and attach the properties enabling simple filtering.     
                tc.TrackEvent(TelemetryHelper.QnAResponseFoundEvent, telemetryProperties);
            });
        }

        /// <summary>
        /// Logs an event indicating that no knowledge (e.g. QNA response or document) could be found for the given question.
        /// Logs an event called "KBQuestion".
        /// </summary>
        /// <param name="context">The conversation context</param>    
        /// <param name="knowledgeBasedUsed">An identifier for the knowledge base used.</param>
        /// <param name="itemsExcluded">An optional list of items that were excluded (e.g. their score/ confidence level was too low).</param>
        public void LogNoKnowledgeForUnknownQuestion(ITurnContext context, string knowledgeBasedUsed = null, Dictionary<string, string> itemsExcluded = null)
        {
            // Simple guard statements
            EnsureNotNull(context, "Context cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                telemetryProperties.Add(TelemetryHelper.QuestionProperty, context.Activity.Text);
                telemetryProperties.Add(TelemetryHelper.FoundInKnowledgeSourceProperty, "False");

                if (!string.IsNullOrEmpty(context.Activity.Conversation.Id))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, context.Activity.Conversation.Id);
                }

                if (!string.IsNullOrEmpty(knowledgeBasedUsed))
                {
                    telemetryProperties.Add(TelemetryHelper.KnowledgeBasedUsedProperty, knowledgeBasedUsed);
                }

                // Store an event called KBQuestion and attach the properties enabling simple filtering.  
                TrackEventWithDataItems(itemsExcluded, TelemetryHelper.KnowledgeItemsDiscardedProperty, TelemetryHelper.KBQuestionEvent, tc, telemetryProperties);
            });
        }

        /// <summary>
        /// Logs an 'Error' event with the given error headline/ text.
        /// Logs an event called "Error".
        /// </summary>
        /// <param name="errorHeadline">The error headline.</param>
        /// <param name="conversationId">The conversation id/</param>
        /// <param name="extraData">Extra data to store about the error.</param>
        public void LogError(string errorHeadline, string conversationId, Dictionary<string, string> extraData = null)
        {
            // Simple guard statements
            EnsureNotNullOrEmpty(errorHeadline, "Property errorHeadline cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                telemetryProperties.Add(TelemetryHelper.ErrorHeadlineProperty, errorHeadline);

                if (!string.IsNullOrEmpty(conversationId))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, conversationId);
                }

                // Store an event called Error and attach the properties enabling simple filtering.  
                TrackEventWithDataItems(extraData, TelemetryHelper.ErrorDataProperty, TelemetryHelper.ErrorEvent, tc, telemetryProperties);
            });
        }

        /// <summary>
        /// Logs an event indicating that a response could not be given to the user.
        /// Logs an event called "NoResponseGiven".
        /// </summary>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="question">The question.</param>
        public void LogNoResponseGiven(string conversationId, string question)
        {
            // Simple guard statements
            EnsureNotNullOrEmpty(question, "Property question cannot be null");

            // Track the event with an exception wrapper
            TrackEventWithExceptionWrapper((TelemetryClient tc) =>
            {
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                telemetryProperties.Add(TelemetryHelper.QuestionProperty, question);

                if (!string.IsNullOrEmpty(conversationId))
                {
                    telemetryProperties.Add(TelemetryHelper.ConversationIdProperty, conversationId);
                }

                // Store an event called NoResponseGiven and attach the properties enabling simple filtering.     
                tc.TrackEvent(TelemetryHelper.NoResponseGivenProperty, telemetryProperties);
            });
        }

        /// <summary>
        /// Internal method with which to add an aggregated telemetry property for the given set of data items.
        /// </summary>
        /// <param name="dataItems">The data items to aggregate.</param>
        /// <param name="dataItemsPropertyName">The property name for the data items.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="tc">The telemetry client.</param>
        /// <param name="telemetryProperties">The telemetry properties.</param>
        private void TrackEventWithDataItems(Dictionary<string, string> dataItems, string dataItemsPropertyName, string eventName, TelemetryClient tc = null, Dictionary<string, string> telemetryProperties = null)
        {
            if (tc == null)
            {
                tc = new TelemetryClient();
            }

            if (dataItems != null && dataItems.Count > 0)
            {
                if (telemetryProperties == null)
                {
                    telemetryProperties = new Dictionary<string, string>();
                }

                // Add a property for the data items which is a comma seperated list of key value pairs, e.g. 'Key=Value, Key=Value'
                string items = dataItems.Select(x => x.Key + "=" + x.Value).Aggregate((s1, s2) => s1 + ", " + s2);
                telemetryProperties.Add(dataItemsPropertyName, items);
            }

            // Track the event
            tc.TrackEvent(eventName, telemetryProperties);
        }

        /// <summary>
        /// Tracks an event with the given exception wrapper (the code to track an event should be in the action passed through).
        /// This ensures that we 'swallow' any exceptions that may interfere with the bot.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        private void TrackEventWithExceptionWrapper(Action<TelemetryClient> action)
        {
            TelemetryClient tc = new TelemetryClient();

            try
            {
                action(tc);
            }
            catch (Exception ex)
            {
                try
                {
                    // catch all general exceptions as we do not want to crash the bot instance
                    tc.TrackException(ex);
                }
                catch
                {
                    // Do nothing if the track exception also fails
                }
            }
        }

        /// <summary>
        /// Ensures the given object is not null.
        /// If it is, throws an argument exception with the given exception string.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="exceptionString">The exception string.</param>
        private void EnsureNotNull(object objectToCheck, string exceptionString)
        {
            if (objectToCheck == null)
            {
                throw new ArgumentException(exceptionString);
            }
        }

        /// <summary>
        /// Ensures the given value is not null or empty.
        /// If it is, throws an argument exception with the given exception string.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="exceptionString">The exception string.</param>
        private static void EnsureNotNullOrEmpty(string value, string exceptionString)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(exceptionString);
            }
        }       
    }

    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        public ApiKeyServiceClientCredentials(string key)
        {
            subscriptionKey = key;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        private string subscriptionKey;
    }
}