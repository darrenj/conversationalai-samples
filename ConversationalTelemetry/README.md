# Conversational Telemetry
We are looking to provide native Telemetry capabilities in a future version of the Bot Framework SDK but here is the code I originally put together to support our first Conversational AI projects.  

This approach uses Application Insights to log an event for every Message received by the Bot and another for any LUIS evaluation. When calling QnAMaker you can log a further event to record finding (or not finding) an answer thus highlighting any knowledge gaps.

[Overview](ConversationalTelemetry/docs/GettingStarted.md)
[Overview](ConversationalTelemetry/docs/PowerBI.md)
[Overview](ConversationalTelemetry/docs/TelemetryCreated.md)

## Contributing

This project welcomes contributions and suggestions.  

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## License

Copyright (c) 2018 Darren Jefford

Licensed under the [MIT](LICENSE.md) License.