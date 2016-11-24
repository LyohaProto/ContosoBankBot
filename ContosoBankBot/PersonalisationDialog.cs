using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoBankBot
{
    [Serializable]
    public class PersonalisationDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await DisplayPersonalisationMenu(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("changepreferredname"))
            {
                PromptDialog.Text(context, AfterUserChangedPreferredName, "Please set a name you would like us to call you: ", "Please try again: ", 2);
            }
            else if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("changehomecity"))
            {
                PromptDialog.Text(context, AfterUserChangedHomeCity, "Please set your home city: ", "Please try again: ", 2);
            }
            else if (message.Text.ToLower(CultureInfo.InvariantCulture).Contains("exit"))
            {
                //Exit
            }
            else
            {
                await DisplayPersonalisationMenu(context);
                context.Wait(MessageReceivedAsync);
            }
        }

        private static async Task DisplayPersonalisationMenu(IDialogContext context)
        {
            IMessageActivity replyToConversation = context.MakeMessage();

            replyToConversation.Type = "message";
            replyToConversation.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>
            {
                new CardAction()
                {
                    Type = "imBack",
                    Title = "Change name",
                    Value = "changepreferredname"
                },
                new CardAction()
                {
                    Type = "imBack",
                    Title = "Change home city",
                    Value = "changehomecity"
                },
                new CardAction()
                {
                    Type = "imBack",
                    Title = "Back to main menu",
                    Value = "exit"
                }
            };

            SigninCard plCard = new SigninCard("Available options", cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            replyToConversation.Attachments.Add(plAttachment);

            await context.PostAsync(replyToConversation);
        }

        private async Task AfterUserChangedPreferredName(IDialogContext context, IAwaitable<string> result)
        {
            var newPreferredName = await result;
            if (!String.IsNullOrEmpty(newPreferredName))
            {
                context.UserData.SetValue("PreferredName", newPreferredName);
                await context.PostAsync($"Nice to meet you, {newPreferredName}!");
            }
            else
            {
                await context.PostAsync("Sorry, we could not change your name. You will have to live with that.");
            }

            await DisplayPersonalisationMenu(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterUserChangedHomeCity(IDialogContext context, IAwaitable<string> result)
        {
            var newHomeCity = await result;
            if (!String.IsNullOrEmpty(newHomeCity))
            {
                context.UserData.SetValue("HomeCity", newHomeCity);
                await context.PostAsync($"The home city has ben set to {newHomeCity}");
            }
            else
            {
                await context.PostAsync("Sorry, we could not change home city. You will have to live with that.");
            }

            await DisplayPersonalisationMenu(context);
            context.Wait(MessageReceivedAsync);
        }
        
    }
}