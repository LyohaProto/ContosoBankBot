using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;


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
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();
            BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
            bool greetingMessageWasShown = userData.GetProperty<bool>("GreetingMessageWasShown");
            string preferredUserName = userData.GetProperty<string>("PreferredName") ?? activity.From.Name;

            Activity reply = activity.CreateReply("Wrong command");

            if (activity.Type == ActivityTypes.Message)
            {
                if (!greetingMessageWasShown)
                {
                    reply = activity.CreateReply($@"Hello, {preferredUserName}!
                                                            I am the Controso Bank Bot!
                                                            I can tell you the latest news from Contoso Bank, show exchange rates, notify about your transactions and do other things!");
                    //connector.Conversations.ReplyToActivityAsync(reply);
                    userData.SetProperty<bool>("GreetingMessageWasShown", true);
                    stateClient.BotState.SetUserData(activity.ChannelId, activity.From.Id, userData);
                }
                else
                {
                    reply = activity.CreateReply($@"{preferredUserName}, You can use standard commands or turn on language recognition mode");
                }

                if (activity.Text == "/deleteprofile")
                {
                    stateClient.BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                    reply = activity.CreateReply("User data was removed");
                }

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                // return our reply to the user 
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
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