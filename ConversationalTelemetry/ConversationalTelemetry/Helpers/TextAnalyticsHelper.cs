using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationalTelemetry.Helpers
{
    static class TextAnalyticsHelper
    {
        static public (string identifiedLanguage, string keyPhrases, double sentiment) EvaluateUtterance(TextAnalyticsClient textAnalyticsClient, string utterance)
        {
            string identifiedLanguage = null;
            string keyPhrases = null;
            double sentiment = Int32.MinValue;

            var detectLanguageResult = textAnalyticsClient.DetectLanguageAsync(new BatchInput(
                  new List<Input>()
                      {
                          new Input("1", utterance),
                  })).Result;

            if (detectLanguageResult != null && detectLanguageResult.Errors.Count == 0)
            {
                identifiedLanguage = detectLanguageResult.Documents[0].DetectedLanguages[0].Name;
                string identfiedIsoLanguage = detectLanguageResult.Documents[0].DetectedLanguages[0].Iso6391Name;

                KeyPhraseBatchResult keyPhraseResult = textAnalyticsClient.KeyPhrasesAsync(new MultiLanguageBatchInput(
                       new List<MultiLanguageInput>()
                       {
                          new MultiLanguageInput(identfiedIsoLanguage, "1", utterance)
                       })).Result;

                if (keyPhraseResult != null && keyPhraseResult.Errors.Count == 0)
                {

                    // Collapse keyphrases into one space delimited string
                    keyPhrases = String.Join(" ", keyPhraseResult.Documents[0].KeyPhrases.Select(x => x.ToString()).ToArray());

                    SentimentBatchResult sentimentResult = textAnalyticsClient.SentimentAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput(identfiedIsoLanguage, "0", utterance)
                        })).Result;

                    if (sentimentResult != null && sentimentResult.Errors.Count == 0)
                    {
                        sentiment = sentimentResult.Documents[0].Score.Value;
                    }
                }               
            }

            return (identifiedLanguage, keyPhrases, sentiment);
        }
    }
}
