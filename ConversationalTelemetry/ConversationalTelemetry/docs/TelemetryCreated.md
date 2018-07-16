# Telemetry Items

## BotMessageReceived

Every message will result in an Application Insights Event being created. This event is called: BotMessageReceived. It will have the following custom dimensions:

	- Channel  (Source channel - e.g. Skype, Cortana, Teams)
	- Text (Contents of the message - may not be populated in extreme circumstances where the customer has asked us not to)
	- FromId
	- FromName
	- ConversationId
	- ConversationName
	- Sentiment (optional but should be there in most cases)
	- Locale
	- Language
	- ClientInfo (most channels will provide this and contents will vary). Skype consumer only has locale, webchat has none, etc.
		○ Here is an example from Teams
		{
		  "locale": "en-GB",
		  "country": "GB",
		  "platform": "Windows"
		}
	
## Intent.INTENTName
Every LUIS Intent hit will result in an Application Insights event being created. This event is called: Intent.INTENTNAME. It will have the following custom dimensions:
	
- ConversationId (for correlation purposes)
- Score
- Question

## UnknownQuestion 

If a question goes to a Knowledge Source an event will be raised to track this including information to help you understand if we found something for the user or if we didn't. The event is called: UnknownQuestion

If we found knowledge the following custom dimensions will be added
- Question
- FoundInKnowledgeSource - set to true
- UserAcceptedAnswer (if the user provided feedback and the developer asked the user) set to true or false based on feedback

If we did not find knowledge for the user the following custom dimensions will be added:
- Question
- FoundInKnowledgeSource - set to false
- KnowledgeItemsDiscarded - if we find items but discard them because the score is too low we add these to help with diagnosis. RecordID and Title will be provided
- Will be provided in the format of ID=Title,ID=Title,ID=Title
