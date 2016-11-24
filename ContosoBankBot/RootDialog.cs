using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoBankBot
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //var message = await argument; //!
            var message = await argument as Activity;
            if (message == null) //!
                context.Wait(MessageReceivedAsync);

            if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("news"))
            {
                var typingMessage = context.MakeMessage();
                typingMessage.Type = ActivityTypes.Typing;
                await context.PostAsync(typingMessage);

                await context.PostAsync(ContosoBankNews.GetLatestNews(message));

                context.Wait(MessageReceivedAsync);
            }
            else if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("office"))
            {
                string homeCity;
                if (!context.UserData.TryGetValue("HomeCity", out homeCity))
                {
                    homeCity = "Auckland";
                    await context.PostAsync($"The home city is not set. Please do it in options. Used default city: {homeCity}");
                }

                var typingMessage = context.MakeMessage();
                typingMessage.Type = ActivityTypes.Typing;
                await context.PostAsync(typingMessage);

                await context.PostAsync(OfficesInfo.GetOfficesLocations(message, homeCity));

                context.Wait(MessageReceivedAsync);
            }
            else if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("rates"))
            {
                var typingMessage = context.MakeMessage();
                typingMessage.Type = ActivityTypes.Typing;
                await context.PostAsync(typingMessage); //!

                await context.PostAsync(CurrenceExchange.GetCurrencyExchangeInfo(message));

                context.Wait(MessageReceivedAsync);
            }
            else if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("personalisation"))
            {
                context.Call(new PersonalisationDialog(), FormDialogComplete);
            }
            else
            {
                await DisplayMainMenu(context);
                context.Wait(MessageReceivedAsync);
            }
        }


        private static async Task DisplayMainMenu(IDialogContext context)
        {
            string displayedUserName;
            if (!context.UserData.TryGetValue("PreferredName", out displayedUserName))
                displayedUserName = "Sir or Madame";

            bool greetingMessageWasShown;
            if (!context.UserData.TryGetValue("GreetingMessageWasShown", out greetingMessageWasShown))
                greetingMessageWasShown = false;

            if (!greetingMessageWasShown)
            {
                await context.PostAsync($@"Hello, {displayedUserName}!
                                           I am the Controso Bank Bot!
                                           I can tell you the latest news from Contoso Bank, show exchange rates, guide you to one of our offices and do other things!");

                context.UserData.SetValue("GreetingMessageWasShown", true);
            }

            IMessageActivity mainMenuMessage = context.MakeMessage();
            mainMenuMessage.Recipient = mainMenuMessage.From;
            mainMenuMessage.Type = "message";
            mainMenuMessage.Attachments = new List<Attachment>();

            List<CardAction> cardButtons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Type = "imBack",
                        Title = "Get latest news",
                        Value = "news"
                    },
                new CardAction()
                {
                    Type = "imBack",
                    Title = "Find offices nearby",
                    Value = "office"
                },
                new CardAction()
                {
                    Type = "imBack",
                    Title = "Foreign exchange rates",
                    Value = "rates"
                },
                new CardAction()
                {
                    Type = "imBack",
                    Title = "Personalisation",
                    Value = "personalisation"
                }
            };

            SigninCard plCard = new SigninCard("Available options", cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            mainMenuMessage.Attachments.Add(plAttachment);

            await context.PostAsync(mainMenuMessage);
        }

        private async Task FormDialogComplete(IDialogContext context, IAwaitable<object> result)
        {
            await DisplayMainMenu(context);
            context.Wait(MessageReceivedAsync);
        }
    }
}