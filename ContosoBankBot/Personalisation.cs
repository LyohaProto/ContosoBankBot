using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoBankBot;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

[Serializable]
public class PersonalisationDialog : IDialog<object>
{
    public async Task StartAsync(IDialogContext context)
    {
        await context.PostAsync($@"Configuration mode");

        IMessageActivity replyToConversation = context.MakeMessage();

        replyToConversation.Type = "message";
        replyToConversation.Attachments = new List<Attachment>();
        List<CardAction> cardButtons = new List<CardAction>();

        cardButtons.Add(
            new CardAction()
            {
                Type = "imBack",
                Title = "Change name",
                Value = "(cash)showatms"
            });

        cardButtons.Add(
            new CardAction()
            {
                Type = "imBack",
                Title = "See transactions",
                Value = "(bow)showoffices"
            });

        cardButtons.Add(
            new CardAction()
            {
                Type = "imBack",
                Title = "Back to main menu",
                Value = "backtomainmenu"
            });

        SigninCard plCard = new SigninCard("Available options", cardButtons);
        Attachment plAttachment = plCard.ToAttachment();
        replyToConversation.Attachments.Add(plAttachment);

        await context.PostAsync(replyToConversation);

        context.Wait(this.MessageReceivedAsync);
    }

    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
    {
        var message = await result as Activity;

        if (message.Text.Equals("backtomainmenu", StringComparison.InvariantCultureIgnoreCase))
        {
            context.Call(new MessagesController.RootDialog(), this.MessageReceivedAsync);
            return;
        }

        Activity replyToConversation = message.CreateReply();
        replyToConversation.Recipient = message.From;
        replyToConversation.Type = "message";
        replyToConversation.Attachments = new List<Attachment>();
        List<CardAction> cardButtons = new List<CardAction>();

        cardButtons.Add(
            new CardAction()
            {
                Type = "imBack",
                Title = "Change name",
                Value = "(cash)showatms"
            });

        cardButtons.Add(
            new CardAction()
            {
                Type = "imBack",
                Title = "See transactions",
                Value = "(bow)showoffices"
            });

        cardButtons.Add(
            new CardAction()
            {
                Type = "imBack",
                Title = "Back to main menu",
                Value = "backtomainmenu"
            });

        SigninCard plCard = new SigninCard("Available options", cardButtons);
        Attachment plAttachment = plCard.ToAttachment();
        replyToConversation.Attachments.Add(plAttachment);

        await context.PostAsync(replyToConversation);

        context.Wait(this.MessageReceivedAsync);
    }
}