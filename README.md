# Wit.ai Bot Framework Integration

## Overview
This is an integration between Wit.ai and Microsoft Bot Framework. Wit.ai allows users to create full fledged dialogs that can contain placeholders for actions, with expected inputs and outputs. This integration will facilitate the implementation of the actions, and automatically paseses the message responses, leveraging the Wit.ai context variables, from Wit.ai bot engine directly to the user.

## Getting Started

* Go to Wit.ai to an existing application, or create a new one.
* Go to Settings tab, and copy the Server Access Token.
* Extend WitDialog class and decorate it like below:
```csharp
[WitModel("Access Token")]
```
* To define action handlers for the actions defined in your Wit.ai application, decorate the handler methods like below:
```csharp
[WitAction("Action Name")]
```
where the action name is how you defined it in your application.

## Use Case and Features
This is useful when you want to create a bot using wit.ai for language understanding and conversation flow. Microsoft Bot Framework is useful for making it easy to publish on several channels and having a good code structure. It's a great place to implement your action and update the wit context variables as well.

## Sample
### Weather App
The WitWeather sample uses [Wit.ai weather application](https://wit.ai/q3blend/weatherApp).

![alt tag](https://i.imgur.com/vtVQAYf.png)

We can see here that "getMyForecast" action needs to be executed. It is expected that location and forecast context variables will be added/updated in "getMyForecast" action like below:

```csharp
[Serializable]
[WitModel("Access Token")]
public class WeatherDialog : WitDialog
```

First, we added the Server Access Token. Now, we need to implement the "getMyForcast" action, which happens here:

```csharp
        [WitAction("getMyForecast")]
        public async Task GetForecast(IDialogContext context, WitResult result)
        {
            //adding location to context
            this.WitContext["location"] =  result.Entities["location"][0].Value;

            //yahoo weather API
            var temp = await GetWeather(this.WitContext["location"]);

            //adding temp to context
            this.WitContext["forecast"] =  temp;
        }
```

## NuGet

https://www.nuget.org/packages/Microsoft.Bot.Builder.Witai/

## More Information
Read these resources for more information about the Microsoft Bot Framework, Bot Builder SDK and Wit.ai Services:

* [Microsoft Bot Framework Overview](https://docs.botframework.com/en-us/)
* [Microsoft Bot Framework Bot Builder SDK](https://github.com/Microsoft/BotBuilder)
* [Microsoft Bot Framework Samples](https://github.com/Microsoft/BotBuilder-Samples)
* [Wit.ai Converse REST Services Documentation](https://wit.ai/docs/http/20160526#post--converse-link)
