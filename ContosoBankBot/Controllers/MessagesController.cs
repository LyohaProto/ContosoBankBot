using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace ContosoBankBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        //private UserInfo _interlocutorUserInfo;

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Text == "/deleteprofile")
                {
                    activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                    Activity reply = activity.CreateReply("User data was removed");

                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                // Try to get username.
                StateClient stateClient = activity.GetStateClient();
                BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
                if (string.IsNullOrEmpty(userData.GetProperty<string>("PreferredName")))
                {
                    if (!string.IsNullOrEmpty(activity.From.Name))
                    {
                        userData.SetProperty("PreferredName", activity.From.Name);
                        stateClient.BotState.SetUserData(activity.ChannelId, activity.From.Id, userData);
                    }
                }

                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        //StateClient stateClient = activity.GetStateClient();
        //BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
        //bool greetingMessageWasShown = userData.GetProperty<bool>("GreetingMessageWasShown");
        //string preferredUserName = userData.GetProperty<string>("PreferredName") ?? activity.From.Name;

        //Activity reply = activity.CreateReply("Wrong command");

        //if (activity.Type == ActivityTypes.Message)
        //{
        //    if (!greetingMessageWasShown)
        //    {
        //        reply = activity.CreateReply($@"Hello, {preferredUserName}!
        //                                                I am the Controso Bank Bot!
        //                                                I can tell you the latest news from Contoso Bank, show exchange rates, notify about your transactions and do other things!");
        //        //connector.Conversations.ReplyToActivityAsync(reply);
        //        userData.SetProperty<bool>("GreetingMessageWasShown", true);
        //        stateClient.BotState.SetUserData(activity.ChannelId, activity.From.Id, userData);
        //    }
        //    else
        //    {
        //        reply = activity.CreateReply($@"{preferredUserName}, You can use standard commands or turn on language recognition mode");
        //    }


        //}
        //else
        //{
        //    HandleSystemMessage(activity);
        //}

        //var response = Request.CreateResponse(HttpStatusCode.OK);
        //return response;



        [Serializable]
        public class RootDialog : IDialog<object>
        {
            private bool greetingMessageWasShown;

            public async Task StartAsync(IDialogContext context)
            {
                string preferredUserName;
                if (!context.UserData.TryGetValue("PreferredName", out preferredUserName))
                {
                    preferredUserName = "Sir or Madame";
                    context.UserData.SetValue("PreferredName", preferredUserName);
                }

                await context.PostAsync($@"Hello, {preferredUserName}!
                                           I am the Controso Bank Bot!
                                           I can tell you the latest news from Contoso Bank, show exchange rates, notify about your transactions and do other things!");

                context.Wait(this.MessageReceivedAsync);
            }

            private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
            {
                var message = await result as Activity;

                string userName;

                if (!context.UserData.TryGetValue("PreferredName", out userName))
                {
                    await context.PostAsync($"I dunno yo name");

                    context.Wait(this.MessageReceivedAsync);
                }

                if (message.Text.Equals("(key)personalisation", StringComparison.InvariantCultureIgnoreCase))
                {
                    context.Call(new PersonalisationDialog(), this.MessageReceivedAsync);
                    return;
                }
                else if (message.Text.StartsWith("call me", StringComparison.InvariantCultureIgnoreCase))
                {
                    var newName = message.Text.Substring("call me".Length).Trim();
                    context.UserData.SetValue("PreferredName", newName);
                    userName = newName;
                }
                else if (message.Text.StartsWith("(flag:US)exchangerates", StringComparison.InvariantCultureIgnoreCase))
                {
                    var typingMessage = context.MakeMessage();
                    typingMessage.Type = ActivityTypes.Typing;
                    await context.PostAsync(typingMessage);
                    await context.PostAsync(GetCurrencyExchangeInfo(message));
                }
                else if (message.Text.StartsWith("getlatestnews", StringComparison.InvariantCultureIgnoreCase))
                {
                    var typingMessage = context.MakeMessage();
                    typingMessage.Type = ActivityTypes.Typing;
                    await context.PostAsync(typingMessage);
                    await context.PostAsync(GetLatestNews(message));
                }
                else if (message.Text.StartsWith("(bow)showoffices", StringComparison.InvariantCultureIgnoreCase))
                {
                    var typingMessage = context.MakeMessage();
                    typingMessage.Type = ActivityTypes.Typing;
                    await context.PostAsync(typingMessage);
                    await context.PostAsync(GetOfficeLocation(message, "Auckland"));
                }
                else
                {
                    Activity replyToConversation = message.CreateReply();
                    replyToConversation.Recipient = message.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardAction> cardButtons = new List<CardAction>();

                    cardButtons.Add(
                        new CardAction()
                        {
                            Type = "imBack",
                            Title = "Get latest news",
                            Value = "getlatestnews"
                        });

                    cardButtons.Add(
                        new CardAction()
                        {
                            Type = "imBack",
                            Title = "(bow) Find offices nearby",
                            Value = "(bow)showoffices"
                        });

                    cardButtons.Add(
                        new CardAction()
                        {
                            Type = "imBack",
                            Title = "(flag:US) Foreign exchange rates",
                            Value = "(flag:US)exchangerates"
                        });

                    cardButtons.Add(
                        new CardAction()
                        {
                            Type = "imBack",
                            Title = "(key) Personalisation",
                            Value = "(key)personalisation"
                        });


                    SigninCard plCard = new SigninCard("Available options", cardButtons);
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);

                    context.UserData.SetValue("GreetingMessageWasShown", true);

                    await context.PostAsync(replyToConversation);
                }

                context.Wait(this.MessageReceivedAsync);
            }

            private IMessageActivity GetLatestNews(Activity message)
            {
                WebRequest request = WebRequest.Create(@"https://contosobank-easytables.azurewebsites.net/tables/BankNews");
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

                var response = (HttpWebResponse)request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string responceData = streamReader.ReadToEnd();
                streamReader.Close();
                response.Close();

                List<News.RootObject> listRootObjects = JsonConvert.DeserializeObject<List<News.RootObject>>(responceData);

                Activity result = message.CreateReply();
                result.Recipient = message.From;
                result.Type = "message";
                result.Attachments = new List<Attachment>();

                foreach (var rootObject in listRootObjects)
                {
                    DateTime newsDate;
                    if (!DateTime.TryParse(rootObject.UpdatedAt, out newsDate))
                        newsDate = DateTime.Today;

                    HeroCard plCard = new HeroCard()
                    {
                        Title = rootObject.Title,
                        Subtitle = newsDate.ToString("d"),
                        Text = rootObject.Text
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    result.Attachments.Add(plAttachment);
                }

                return result;
            }

            private Activity GetCurrencyExchangeInfo(Activity message)
            {
                WebRequest request = WebRequest.Create(@"http://api.fixer.io/latest?base=NZD&symbols=AUD,USD,GBP,RUB");
                var response = (HttpWebResponse)request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string responceData = streamReader.ReadToEnd();
                streamReader.Close();
                response.Close();

                RatesRootObject r = JsonConvert.DeserializeObject<RatesRootObject>(responceData);

                DateTime rateDate;
                if (!DateTime.TryParse(r.date, out rateDate))
                    rateDate = DateTime.Today;

                Activity result = message.CreateReply();
                result.Recipient = message.From;
                result.Type = "message";
                result.Attachments = new List<Attachment>();

                HeroCard plCard = new HeroCard()
                {
                    Title = "Foreign currency exchange rates",
                    Subtitle = rateDate.ToString("d"),
                    Text = $"1 NZD = {r.rates.USD} USD\n     {r.rates.AUD} AUD\n     {r.rates.GBP} GBP\n     {r.rates.RUB} RUB"
                };
                Attachment plAttachment = plCard.ToAttachment();
                result.Attachments.Add(plAttachment);

                return result;
            }

            private Activity GetOfficeLocation(Activity message, string city)
            {
                WebRequest request = WebRequest.Create(@"https://contosobank-easytables.azurewebsites.net/tables/OfficeLocations");
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

                var response = (HttpWebResponse)request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string responceData = streamReader.ReadToEnd();
                streamReader.Close();
                response.Close();

                List<OfficeAddress.RootObject> listRootObjects = JsonConvert.DeserializeObject<List<OfficeAddress.RootObject>>(responceData);

                Activity result = message.CreateReply();
                result.Recipient = message.From;
                result.Type = "message";
                result.Attachments = new List<Attachment>();

                foreach (var rootObject in listRootObjects)
                {
                    if (rootObject.City.ToLower() == city.ToLower())
                    {
                        List<CardAction> cardButtons = new List<CardAction>();
                        result.Attachments.Add(new HeroCard()
                        {
                            Title = "Office in " + rootObject.City,
                            Subtitle = rootObject.Location,
                            Text = "Phone: " + rootObject.Phone,
                            Buttons = cardButtons
                        }.ToAttachment());

                        cardButtons.Add(new CardAction()
                        {
                            Value = @"https://www.bing.com/maps?rtp=~adr.One" + rootObject.Location.Replace(" ", "%20") + @"&style=r&lvl=16&trfc=1",
                            Type = "openUrl",
                            Title = "Open on Bing Maps"
                        });

                        cardButtons.Add(new CardAction()
                        {
                            Value = rootObject.Phone,
                            Type = "call",
                            Title = "Call office"
                        });
                    }
                }

                return result;
            }
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
                //_interlocutorUserInfo.Dispose();
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }


    public class UserInfo : IDisposable
    {
        private readonly StateClient _stateClient;
        private readonly BotData _userData;
        private readonly Activity _activity;

        public UserInfo(Activity message)
        {
            _activity = message;
            _stateClient = _activity.GetStateClient();
            _userData = _stateClient.BotState.GetUserData(_activity.ChannelId, _activity.From.Id);
        }

        private void SaveUserData()
        {
            _stateClient.BotState.SetUserData(_activity.ChannelId, _activity.From.Id, _userData);
        }

        public string PreferredName
        {
            get
            {
                return _userData.GetProperty<string>("PreferredName") ?? "Sir or Madam";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "Sir or Madam";
                }
                _userData.SetProperty("PreferredName", value);
                SaveUserData();
            }
        }

        public bool GreetingMessageWasShown
        {
            get
            {
                return _userData.GetProperty<bool>("GreetingMessageWasShown");
            }
            set
            {
                _userData.SetProperty("GreetingMessageWasShown", value);
                SaveUserData();
            }
        }

        public void Dispose()
        {
            _stateClient.BotState.DeleteStateForUser(_activity.ChannelId, _activity.From.Id);
        }
    }
}